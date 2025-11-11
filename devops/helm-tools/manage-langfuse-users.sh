#!/bin/bash
set -euo pipefail

# Interactive Langfuse User Management Script
# Allows viewing, deleting specific users, or deleting all non-admin users

NAMESPACE="devops-tools"
PG_HOST="dev-pg-zionet-learning.postgres.database.azure.com"
ENVIRONMENT_NAME="${1:-dev}"
PG_USERNAME="${2:-postgres}"
PG_PASSWORD="${3:-postgres}"

# Detect if running in WSL and use Windows kubectl if needed
if grep -qi microsoft /proc/version 2>/dev/null; then
  # Running in WSL - use kubectl.exe from Windows
  KUBECTL="kubectl.exe"
else
  # Running in native Linux or Git Bash
  KUBECTL="kubectl"
fi

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to display users
show_users() {
  echo ""
  echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
  echo -e "${GREEN}📋 LANGFUSE USERS - Environment: $ENVIRONMENT_NAME${NC}"
  echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
  echo ""

  echo "Loading users..."
  
  $KUBECTL run -n $NAMESPACE temp-list-users-$$ --image=postgres:16 --rm -i --restart=Never -- \
    psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
    -c "
      SELECT 
        ROW_NUMBER() OVER (ORDER BY u.created_at DESC) as \"#\",
        u.email as \"Email\",
        u.name as \"Name\",
        CASE WHEN u.admin THEN 'YES' ELSE 'NO' END as \"Is Admin\",
        CASE WHEN u.email_verified IS NOT NULL THEN 'YES' ELSE 'NO' END as \"Verified\",
        u.created_at::date as \"Created\",
        COALESCE(org_info.organizations, 'None') as \"Organizations\",
        COALESCE(org_info.roles, 'None') as \"Roles\"
      FROM users u
      LEFT JOIN (
        SELECT 
          om.user_id,
          STRING_AGG(o.name, ', ' ORDER BY o.name) as organizations,
          STRING_AGG(om.role::text, ', ' ORDER BY o.name) as roles
        FROM organization_memberships om
        JOIN organizations o ON om.org_id = o.id
        GROUP BY om.user_id
      ) org_info ON org_info.user_id = u.id
      ORDER BY u.created_at DESC;
    "

  if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Failed to retrieve users. Check your database connection.${NC}"
  fi

  echo ""
  echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

# Function to delete a specific user
delete_user() {
  local user_email="$1"
  
  echo ""
  echo -e "${YELLOW}🔍 Checking if user exists: $user_email${NC}"

  USER_INFO=$($KUBECTL run -n $NAMESPACE temp-check-user-$$ --image=postgres:16 --rm -i --restart=Never -- \
    psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
    -v email="$user_email" \
    -t -c "
      SELECT 
        email, 
        name, 
        CASE WHEN admin THEN 'YES' ELSE 'NO' END as is_admin,
        (SELECT COUNT(*) FROM organization_memberships WHERE user_id = users.id) as org_count
      FROM users 
      WHERE email = :'email';
    " | xargs)

  if [ -z "$USER_INFO" ]; then
    echo -e "${RED}❌ User not found: $user_email${NC}"
    return 1
  fi

  echo -e "${GREEN}✅ User found:${NC}"
  echo "   $USER_INFO"
  echo ""

  # Check if user is admin
  if echo "$USER_INFO" | grep -q "YES"; then
    echo -e "${RED}⚠️  WARNING: This user has ADMIN privileges!${NC}"
  fi

  read -p "$(echo -e ${YELLOW}Are you sure you want to delete this user? Type 'DELETE' to confirm: ${NC})" CONFIRM

  if [ "$CONFIRM" != "DELETE" ]; then
    echo -e "${YELLOW}❌ Deletion cancelled.${NC}"
    return 1
  fi

  echo -e "${BLUE}🗑️  Deleting user: $user_email${NC}"

  $KUBECTL run -n $NAMESPACE temp-delete-user-$$ --image=postgres:16 --rm -i --restart=Never -- \
    psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
    -v email="$user_email" \
    -c "
      DO \$BODY\$
      DECLARE
          v_user_id text;
          v_email text := :'email';
      BEGIN
          SELECT id INTO v_user_id FROM users WHERE email = v_email;
          
          IF v_user_id IS NULL THEN
              RAISE EXCEPTION 'User not found: %', v_email;
          END IF;

          -- Delete organization memberships
          DELETE FROM organization_memberships WHERE user_id = v_user_id;
          RAISE NOTICE 'Deleted organization memberships';
          
          -- Delete API keys (if table exists and has user_id column)
          BEGIN
              DELETE FROM api_keys WHERE user_id = v_user_id;
              RAISE NOTICE 'Deleted API keys';
          EXCEPTION 
              WHEN undefined_table THEN
                  RAISE NOTICE 'api_keys table does not exist, skipping';
              WHEN undefined_column THEN
                  RAISE NOTICE 'api_keys table exists but user_id column not found, skipping';
          END;
          
          -- Delete from other related tables that might exist
          -- These will fail silently if tables don't exist
          BEGIN
              DELETE FROM project_memberships WHERE user_id = v_user_id;
              RAISE NOTICE 'Deleted project memberships';
          EXCEPTION WHEN undefined_table THEN
              RAISE NOTICE 'project_memberships table does not exist, skipping';
          END;
          
          BEGIN
              DELETE FROM sessions WHERE user_id = v_user_id;
              RAISE NOTICE 'Deleted sessions';
          EXCEPTION WHEN undefined_table THEN
              RAISE NOTICE 'sessions table does not exist, skipping';
          END;
          
          BEGIN
              DELETE FROM cloud_configs WHERE user_id = v_user_id;
              RAISE NOTICE 'Deleted cloud configs';
          EXCEPTION WHEN undefined_table THEN
              RAISE NOTICE 'cloud_configs table does not exist, skipping';
          END;

          -- Finally, delete the user account
          DELETE FROM users WHERE id = v_user_id;
          RAISE NOTICE 'User deleted successfully: %', v_email;
      END \$BODY\$;
    "

  echo ""
  echo -e "${GREEN}✅ User deleted successfully: $user_email${NC}"
}

# Function to delete all non-admin users
delete_all_non_admin() {
  echo ""
  echo -e "${YELLOW}⚠️  WARNING: This will delete ALL non-admin users!${NC}"
  echo ""
  
  # Show non-admin users that will be deleted
  echo -e "${BLUE}The following users will be deleted:${NC}"
  $KUBECTL run -n $NAMESPACE temp-show-nonadmin-$$ --image=postgres:16 --rm -i --restart=Never -- \
    psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
    -c "
      SELECT email, name, created_at::date as created
      FROM users 
      WHERE admin = false
      ORDER BY created_at DESC;
    "

  echo ""
  read -p "$(echo -e ${RED}Type 'DELETE ALL NON-ADMIN' to confirm deletion: ${NC})" CONFIRM

  if [ "$CONFIRM" != "DELETE ALL NON-ADMIN" ]; then
    echo -e "${YELLOW}❌ Deletion cancelled.${NC}"
    return 1
  fi

  echo -e "${BLUE}🗑️  Deleting all non-admin users...${NC}"

  $KUBECTL run -n $NAMESPACE temp-delete-nonadmin-$$ --image=postgres:16 --rm -i --restart=Never -- \
    psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
    -c "
      DO \$BODY\$
      DECLARE
          deleted_count integer;
      BEGIN
          -- Delete organization memberships for non-admin users
          DELETE FROM organization_memberships WHERE user_id IN (SELECT id FROM users WHERE admin = false);
          RAISE NOTICE 'Deleted organization memberships';
          
          -- Delete API keys for non-admin users (if table exists and has user_id column)
          BEGIN
              DELETE FROM api_keys WHERE user_id IN (SELECT id FROM users WHERE admin = false);
              RAISE NOTICE 'Deleted API keys';
          EXCEPTION 
              WHEN undefined_table THEN
                  RAISE NOTICE 'api_keys table does not exist, skipping';
              WHEN undefined_column THEN
                  RAISE NOTICE 'api_keys table exists but user_id column not found, skipping';
          END;
          
          -- Delete from other related tables that might exist
          BEGIN
              DELETE FROM project_memberships WHERE user_id IN (SELECT id FROM users WHERE admin = false);
              RAISE NOTICE 'Deleted project memberships';
          EXCEPTION WHEN undefined_table THEN
              RAISE NOTICE 'project_memberships table does not exist, skipping';
          END;
          
          BEGIN
              DELETE FROM sessions WHERE user_id IN (SELECT id FROM users WHERE admin = false);
              RAISE NOTICE 'Deleted sessions';
          EXCEPTION WHEN undefined_table THEN
              RAISE NOTICE 'sessions table does not exist, skipping';
          END;
          
          BEGIN
              DELETE FROM cloud_configs WHERE user_id IN (SELECT id FROM users WHERE admin = false);
              RAISE NOTICE 'Deleted cloud configs';
          EXCEPTION WHEN undefined_table THEN
              RAISE NOTICE 'cloud_configs table does not exist, skipping';
          END;
          
          -- Delete non-admin users and count them
          DELETE FROM users WHERE admin = false;
          GET DIAGNOSTICS deleted_count = ROW_COUNT;
          
          RAISE NOTICE 'Deleted % non-admin users', deleted_count;
      END \$BODY\$;

      -- Show remaining users (should be only admins)
      SELECT 
        COUNT(*) as remaining_users,
        COUNT(*) FILTER (WHERE admin = true) as admin_users,
        COUNT(*) FILTER (WHERE admin = false) as non_admin_users
      FROM users;
    "

  echo ""
  echo -e "${GREEN}✅ All non-admin users have been deleted${NC}"
}

# Main menu
show_menu() {
  echo ""
  echo -e "${GREEN}╔════════════════════════════════════════════════════════╗${NC}"
  echo -e "${GREEN}║        LANGFUSE USER MANAGEMENT MENU                   ║${NC}"
  echo -e "${GREEN}╚════════════════════════════════════════════════════════╝${NC}"
  echo ""
  echo "  1) Show all users"
  echo "  2) Delete specific user by email"
  echo "  3) Delete all non-admin users"
  echo "  4) Exit"
  echo ""
}

# Main loop
main() {
  echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
  echo -e "${GREEN}🔐 Langfuse User Management Tool${NC}"
  echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
  echo -e "Environment: ${YELLOW}$ENVIRONMENT_NAME${NC}"
  echo -e "Database: ${YELLOW}langfuse-$ENVIRONMENT_NAME${NC}"
  echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

  # Show users initially
  show_users

  while true; do
    show_menu
    read -p "Select an option (1-4): " choice

    case $choice in
      1)
        show_users
        ;;
      2)
        echo ""
        read -p "Enter the email address of the user to delete: " user_email
        if [ -n "$user_email" ]; then
          delete_user "$user_email"
        else
          echo -e "${RED}❌ Email cannot be empty${NC}"
        fi
        ;;
      3)
        delete_all_non_admin
        show_users
        ;;
      4)
        echo ""
        echo -e "${GREEN}👋 Exiting user management tool. Goodbye!${NC}"
        echo ""
        exit 0
        ;;
      *)
        echo -e "${RED}❌ Invalid option. Please select 1-4.${NC}"
        ;;
    esac
  done
}

# Run main function
main
