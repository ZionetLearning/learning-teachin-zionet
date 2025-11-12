#!/bin/bash
set -euo pipefail

# Detect $KUBECTL command (use .exe version in WSL)
if command -v kubectl.exe &> /dev/null; then
  KUBECTL="kubectl.exe"
else
  KUBECTL="kubectl"
fi

# Script to create a new Langfuse user and send credentials via email
# Usage: ./create-langfuse-user.sh <user_email> <user_name> [environment]

NAMESPACE="devops-tools"
PG_HOST="dev-pg-zionet-learning.postgres.database.azure.com"

# Check if required parameters are provided
if [ $# -lt 2 ]; then
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "❌ Error: Missing required parameters"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo ""
  echo "Usage: $0 <user_email> <user_name> [environment] [pg_username] [pg_password]"
  echo ""
  echo "Required:"
  echo "  user_email    - Email address for the new user"
  echo "  user_name     - Full name of the user"
  echo ""
  echo "Optional:"
  echo "  environment   - Environment name (default: dev)"
  echo "  pg_username   - PostgreSQL username (default: postgres)"
  echo "  pg_password   - PostgreSQL password (default: postgres)"
  echo ""
  echo "Environment Variables Required:"
  echo "  SMTP_USER     - SMTP email address (e.g., your-email@gmail.com)"
  echo "  SMTP_PASSWORD - SMTP password (Gmail App Password)"
  echo ""
  echo "Examples:"
  echo "  # Set SMTP credentials first"
  echo "  export SMTP_USER='admin@teachin.local'"
  echo "  export SMTP_PASSWORD='your-app-password'"
  echo ""
  echo "  # Create a user with defaults"
  echo "  $0 'john.doe@example.com' 'John Doe'"
  echo ""
  echo "  # Create a user with custom environment"
  echo "  $0 'jane.smith@example.com' 'Jane Smith' prod postgres mypassword"
  echo ""
  exit 1
fi

USER_EMAIL="$1"
USER_NAME="$2"
ENVIRONMENT_NAME="${3:-dev}"
PG_USERNAME="${4:-postgres}"
PG_PASSWORD="${5:-postgres}"
HIDE_PASSWORD="${6:-false}"  # Set to 'true' in CI/CD to hide password

# Generate a random password (12 characters: letters, numbers, and special chars)
GENERATED_PASSWORD=$(openssl rand -base64 12 | tr -d "=+/" | cut -c1-12)

# SMTP Configuration (same as langfuse.sh)
SMTP_HOST="smtp.gmail.com"
SMTP_PORT="465"
SMTP_USER="${SMTP_USER:-}"
SMTP_PASSWORD="${SMTP_PASSWORD:-}"
FROM_EMAIL="${SMTP_USER}"
FROM_NAME="TeachIn Admin"

if [ -z "$SMTP_USER" ] || [ -z "$SMTP_PASSWORD" ]; then
  echo "❌ SMTP credentials not set. Please set SMTP_USER and SMTP_PASSWORD environment variables."
  echo "   Example: export SMTP_USER='your-email@gmail.com'"
  echo "           export SMTP_PASSWORD='your-app-password'"
  exit 1
fi

echo "👤 Creating Langfuse user: $USER_EMAIL"
echo "📧 Name: $USER_NAME"
echo "🗄️  Database: langfuse-$ENVIRONMENT_NAME"
if [ "$HIDE_PASSWORD" = "true" ]; then
  echo "🔑 Generated Password: ********** (hidden for security)"
else
  echo "🔑 Generated Password: $GENERATED_PASSWORD"
fi

# Generate bcrypt hash for the password
echo "🔐 Generating bcrypt hash..."

$KUBECTL delete job user-hash-generator -n "$NAMESPACE" --ignore-not-found=true
sleep 1

cat <<EOF | $KUBECTL apply -f -
apiVersion: batch/v1
kind: Job
metadata:
  name: user-hash-generator
  namespace: $NAMESPACE
spec:
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: hash-gen
        image: python:3.11-alpine
        command: ["/bin/sh", "-c"]
        args:
        - |
          pip install bcrypt >/dev/null 2>&1
          python3 -c "
          import bcrypt
          password = '$GENERATED_PASSWORD'
          salt = bcrypt.gensalt(rounds=12, prefix=b'2a')
          hash_value = bcrypt.hashpw(password.encode('utf-8'), salt)
          print('HASH:' + hash_value.decode('utf-8'))
          "
EOF

$KUBECTL wait --for=condition=complete job/user-hash-generator -n "$NAMESPACE" --timeout=60s
HASH=$($KUBECTL logs job/user-hash-generator -n "$NAMESPACE" | grep "HASH:" | cut -d: -f2)
$KUBECTL delete job user-hash-generator -n "$NAMESPACE"

if [ -z "$HASH" ] || [ ${#HASH} -lt 20 ]; then
  echo "❌ Failed to generate password hash. Cannot proceed."
  exit 1
fi

echo "✅ Password hash generated"

# Create user in database
echo "💾 Creating user in database..."

$KUBECTL run -n $NAMESPACE temp-create-user --image=postgres:16 --rm -i --restart=Never -- \
  psql "host=$PG_HOST port=5432 dbname=langfuse-${ENVIRONMENT_NAME} user=$PG_USERNAME password=$PG_PASSWORD sslmode=require" \
  -c "
    -- Create the user (without organization membership)
    INSERT INTO users (id, name, email, password, admin, email_verified, created_at, updated_at)
    VALUES (gen_random_uuid()::text, '$USER_NAME', '$USER_EMAIL', '$HASH', false, NOW(), NOW(), NOW())
    ON CONFLICT (email) DO UPDATE
      SET password = EXCLUDED.password,
          name = EXCLUDED.name,
          email_verified = NOW(),
          updated_at = NOW();

    -- Display user info
    SELECT 'User created:' as message, email, name, admin, email_verified
    FROM users
    WHERE email = '$USER_EMAIL';
  "

echo "✅ User created in database"

# Send email with credentials
echo "📧 Sending credentials via email..."

# Create email sending job
$KUBECTL delete job email-sender -n "$NAMESPACE" --ignore-not-found=true
sleep 1

$KUBECTL run email-sender \
  -n "$NAMESPACE" \
  --image=python:3.11-alpine \
  --restart=Never \
  --rm -i \
  --env="SMTP_HOST=$SMTP_HOST" \
  --env="SMTP_PORT=$SMTP_PORT" \
  --env="SMTP_USER=$SMTP_USER" \
  --env="SMTP_PASSWORD=$SMTP_PASSWORD" \
  --env="FROM_NAME=$FROM_NAME" \
  --env="TO_EMAIL=$USER_EMAIL" \
  --env="TO_NAME=$USER_NAME" \
  --env="USER_PASSWORD=$GENERATED_PASSWORD" \
  -- sh -c '
pip install -q --no-cache-dir --disable-pip-version-check 2>&1 | grep -v "WARNING" || true

python3 << "PYEOF"
import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
import os
import sys

smtp_host = os.environ["SMTP_HOST"]
smtp_port = int(os.environ["SMTP_PORT"])
smtp_user = os.environ["SMTP_USER"]
smtp_password = os.environ["SMTP_PASSWORD"]
from_name = os.environ["FROM_NAME"]
to_email = os.environ["TO_EMAIL"]
to_name = os.environ["TO_NAME"]
user_password = os.environ["USER_PASSWORD"]

html_body = f"""<!DOCTYPE html>
<html>
<head>
  <style>
    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
    .header {{ background: #667eea; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
    .credentials {{ background: white; padding: 20px; border-left: 4px solid #667eea; margin: 20px 0; }}
    .button {{ background: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
    .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    code {{ background: #f4f4f4; padding: 2px 6px; border-radius: 3px; font-family: monospace; }}
  </style>
</head>
<body>
  <div class="container">
    <div class="header">
      <h1>Welcome to TeachIn Langfuse</h1>
    </div>
    <div class="content">
      <p>Hello <strong>{to_name}</strong>,</p>
      
      <p>Your Langfuse account has been created successfully. Below are your login credentials:</p>
      
      <div class="credentials">
        <p><strong>Email:</strong> <code>{to_email}</code></p>
        <p><strong>Password:</strong> <code>{user_password}</code></p>
        <p><strong>Login URL:</strong> <a href="https://teachin.westeurope.cloudapp.azure.com/langfuse">https://teachin.westeurope.cloudapp.azure.com/langfuse</a></p>
      </div>
      
      <p>
        <a href="https://teachin.westeurope.cloudapp.azure.com/langfuse/auth/sign-in" class="button">
          Sign In Now
        </a>
      </p>
      
      <p><strong>⚠️ Important Security Notice:</strong></p>
      <ul>
        <li>Please change your password after your first login</li>
        <li>Do not share your credentials with anyone</li>
        <li>Keep this email secure or delete it after changing your password</li>
      </ul>
      
      <p>If you have any questions or need assistance, please contact our support team.</p>
      
      <p>Best regards,<br><strong>TeachIn Admin Team</strong></p>
    </div>
    <div class="footer">
      <p>This is an automated message. Please do not reply to this email.</p>
    </div>
  </div>
</body>
</html>"""

try:
    message = MIMEMultipart("alternative")
    message["Subject"] = "Your Langfuse Account Credentials - TeachIn"
    message["From"] = f"{from_name} <{smtp_user}>"
    message["To"] = to_email
    
    html_part = MIMEText(html_body, "html")
    message.attach(html_part)
    
    print(f"Connecting to {smtp_host}:{smtp_port}...")
    server = smtplib.SMTP_SSL(smtp_host, smtp_port)
    server.login(smtp_user, smtp_password)
    
    print(f"Sending email to {to_email}...")
    server.send_message(message)
    server.quit()
    
    print(f"✅ Email sent successfully to {to_email}")
    sys.exit(0)
    
except Exception as e:
    print(f"❌ Failed to send email: {str(e)}")
    import traceback
    traceback.print_exc()
    sys.exit(1)
PYEOF
'

EMAIL_EXIT_CODE=$?

if [ "$EMAIL_EXIT_CODE" != "0" ]; then
  echo "❌ Failed to send email"
  echo "⚠️  User was created but email notification failed"
  if [ "$HIDE_PASSWORD" != "true" ]; then
    echo "📋 Please manually provide credentials to user:"
    echo "   Email: $USER_EMAIL"
    echo "   Password: $GENERATED_PASSWORD"
  fi
  exit 1
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ User created and email sent successfully!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "👤 Name: $USER_NAME"
echo "📧 Email: $USER_EMAIL"
if [ "$HIDE_PASSWORD" = "true" ]; then
  echo "🔑 Password: ********** (sent to user via email)"
else
  echo "🔑 Password: $GENERATED_PASSWORD"
fi
echo "🔗 Login URL: https://teachin.westeurope.cloudapp.azure.com/langfuse"
echo "📬 Email sent to: $USER_EMAIL"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ "$HIDE_PASSWORD" != "true" ]; then
  echo "⚠️  Remind the user to change their password after first login"
else
  echo "✅ Password has been securely sent to the user's email"
fi
echo ""
