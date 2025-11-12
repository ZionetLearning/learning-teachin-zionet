#!/bin/bash
set -euo pipefail

# Non-interactive Langfuse User Management Script for CI/CD
# Usage: ./manage-langfuse-users-ci.sh <action> [user_email] [environment] [pg_username] [pg_password]

NAMESPACE="devops-tools"
PG_HOST="dev-pg-zionet-learning.postgres.database.azure.com"

# Detect if running in WSL and use Windows kubectl if needed
if grep -qi microsoft /proc/version 2>/dev/null; then
  KUBECTL="kubectl.exe"
else
  KUBECTL="kubectl"
fi

# Parse arguments
ACTION="${1:-}"
USER_EMAIL="${2:-}"
ENVIRONMENT_NAME="${3:-dev}"
PG_USERNAME="${4:-postgres}"
PG_PASSWORD="${5:-postgres}"

if [ -z "$ACTION" ]; then
  echo "Usage: $0 <action> [user_email] [environment] [pg_username] [pg_password]"
  echo ""
  echo "Actions:"
  echo "  list-users                - Show all users"
  echo "  delete-user <email>       - Delete specific user"
  echo "  delete-all-non-admin      - Delete all non-admin users"
  echo ""
  echo "Examples:"
  echo "  $0 list-users"
  echo "  $0 delete-user 'test@example.com'"
  echo "  $0 delete-all-non-admin"
  exit 1
fi

# Function to list users
list_users() {
  echo "📋 Listing Langfuse users - Environment: $ENVIRONMENT_NAME"
  echo ""

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
}

# Function to delete a specific user
delete_user() {
  if [ -z "$USER_EMAIL" ]; then
    echo "❌ Error: user_email is required for delete-user action"
    exit 1
  fi

  echo "🔍 Checking user: $USER_EMAIL"
  
  # Check if user exists
  $KUBECTL run -n $NAMESPACE temp-check-user-$$ --image=postgres:16 --rm -i --restart=Never --env="USER_EMAIL=$USER_EMAIL" -- \
    bash -c "psql \"host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require\" \
    -c \"
      SELECT 
        email, 
        name, 
        CASE WHEN admin THEN 'ADMIN' ELSE 'USER' END as type,
        (SELECT COUNT(*) FROM organization_memberships WHERE user_id = users.id) as org_count
      FROM users 
      WHERE email = '\$USER_EMAIL';
    \""

  echo ""
  echo "🗑️  Deleting user: $USER_EMAIL"
  
  # Delete user
  $KUBECTL run -n $NAMESPACE temp-delete-user-$$ --image=postgres:16 --rm -i --restart=Never --env="USER_EMAIL=$USER_EMAIL" -- \
    bash -c "psql \"host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require\" \
    -c \"
      DO \\\$\\\$
      DECLARE
          v_user_id text;
      BEGIN
          SELECT id INTO v_user_id FROM users WHERE email = '\$USER_EMAIL';
          
          IF v_user_id IS NULL THEN
              RAISE EXCEPTION 'User not found: %', '\$USER_EMAIL';
          END IF;

          -- Delete organization memberships
          DELETE FROM organization_memberships WHERE user_id = v_user_id;
          
          -- Delete API keys (if table exists)
          BEGIN
              DELETE FROM api_keys WHERE user_id = v_user_id;
          EXCEPTION 
              WHEN undefined_table THEN NULL;
              WHEN undefined_column THEN NULL;
          END;
          
          -- Delete project memberships (if exists)
          BEGIN
              DELETE FROM project_memberships WHERE user_id = v_user_id;
          EXCEPTION WHEN undefined_table THEN NULL;
          END;
          
          -- Delete sessions (if exists)
          BEGIN
              DELETE FROM sessions WHERE user_id = v_user_id;
          EXCEPTION WHEN undefined_table THEN NULL;
          END;
          
          -- Delete cloud configs (if exists)
          BEGIN
              DELETE FROM cloud_configs WHERE user_id = v_user_id;
          EXCEPTION WHEN undefined_table THEN NULL;
          END;

          -- Finally, delete the user
          DELETE FROM users WHERE id = v_user_id;
          RAISE NOTICE 'User deleted successfully: %', '\$USER_EMAIL';
      END \\\$\\\$;
    \""

  echo ""
  echo "✅ User deleted successfully: $USER_EMAIL"
}

# Function to delete all non-admin users
delete_all_non_admin() {
  echo "⚠️  Deleting all non-admin users - Environment: $ENVIRONMENT_NAME"
  echo ""
  
  # Show users that will be deleted
  echo "Users to be deleted:"
  $KUBECTL run -n $NAMESPACE temp-show-nonadmin-$$ --image=postgres:16 --rm -i --restart=Never -- \
    psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
    -c "
      SELECT email, name, created_at::date as created
      FROM users 
      WHERE admin = false
      ORDER BY created_at DESC;
    "

  echo ""
  echo "🗑️  Deleting users..."
  
  # Delete all non-admin users
  $KUBECTL run -n $NAMESPACE temp-delete-nonadmin-$$ --image=postgres:16 --rm -i --restart=Never -- \
    psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
    -c "
      DO \$BODY\$
      DECLARE
          deleted_count integer;
      BEGIN
          -- Delete organization memberships
          DELETE FROM organization_memberships WHERE user_id IN (SELECT id FROM users WHERE admin = false);
          
          -- Delete API keys (if exists)
          BEGIN
              DELETE FROM api_keys WHERE user_id IN (SELECT id FROM users WHERE admin = false);
          EXCEPTION 
              WHEN undefined_table THEN NULL;
              WHEN undefined_column THEN NULL;
          END;
          
          -- Delete project memberships (if exists)
          BEGIN
              DELETE FROM project_memberships WHERE user_id IN (SELECT id FROM users WHERE admin = false);
          EXCEPTION WHEN undefined_table THEN NULL;
          END;
          
          -- Delete sessions (if exists)
          BEGIN
              DELETE FROM sessions WHERE user_id IN (SELECT id FROM users WHERE admin = false);
          EXCEPTION WHEN undefined_table THEN NULL;
          END;
          
          -- Delete cloud configs (if exists)
          BEGIN
              DELETE FROM cloud_configs WHERE user_id IN (SELECT id FROM users WHERE admin = false);
          EXCEPTION WHEN undefined_table THEN NULL;
          END;
          
          -- Delete non-admin users
          DELETE FROM users WHERE admin = false;
          GET DIAGNOSTICS deleted_count = ROW_COUNT;
          
          RAISE NOTICE 'Deleted % non-admin users', deleted_count;
      END \$BODY\$;

      -- Show remaining users
      SELECT 
        COUNT(*) as total_remaining,
        COUNT(*) FILTER (WHERE admin = true) as admin_users,
        COUNT(*) FILTER (WHERE admin = false) as non_admin_users
      FROM users;
    "

  echo ""
  echo "✅ All non-admin users have been deleted"
}

# Execute action
case "$ACTION" in
  list-users)
    list_users
    ;;
  delete-user)
    delete_user
    ;;
  delete-all-non-admin)
    delete_all_non_admin
    ;;
  *)
    echo "❌ Invalid action: $ACTION"
    echo "Valid actions: list-users, delete-user, delete-all-non-admin"
    exit 1
    ;;
esac
