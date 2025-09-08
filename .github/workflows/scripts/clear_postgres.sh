#!/usr/bin/env bash
set -euo pipefail

echo "[clear_postgres] Starting"

: "${PGHOST:?PGHOST required}"
: "${PGDATABASE:?PGDATABASE required}"
: "${PGUSER:?PGUSER required}"
: "${PGPASSWORD:?PGPASSWORD required}"
#: "${PGPORT:?PGPORT required}"
: "${MODE:?MODE required}"
: "${DRY_RUN:?DRY_RUN required}"
: "${VERIFY_SSL:?VERIFY_SSL required}"

if [[ "${VERIFY_SSL}" == "true" ]]; then
  export PGSSLMODE=require
  echo "[clear_postgres] SSL mode: require"
else
  export PGSSLMODE=disable
  echo "::warning::SSL disabled (NOT recommended)."
fi

# grep -qi:
#   grep : search text for a pattern.
#   -q  : quiet (no output, only exit status).
#   -i  : case-insensitive match.
# Here-string <<< feeds the variable's value as stdin to grep.
if ! grep -qi '\.postgres\.database\.azure\.com$' <<<"$PGHOST"; then
  echo "::warning::Host does not look like Azure Flexible Server (*.postgres.database.azure.com)"
fi

# Variable interpolation $VAR inside double quotes; prints connection target.
echo "[clear_postgres] Target: $PGHOST/$PGDATABASE (user: $PGUSER)"

# psql:
#   -v ON_ERROR_STOP=1 : stop executing further commands on first SQL error.
#   -c "SQL" : execute the provided SQL command and exit (can repeat).
echo "[clear_postgres] Connectivity / SSL check..."
psql -v ON_ERROR_STOP=1 -c "SELECT version();" -c "SHOW ssl;" -c "SELECT current_user, current_database();"

# Validate MODE content with a compound conditional using && (and) plus [[ ... ]].
if [[ "$MODE" != "drop" && "$MODE" != "truncate" ]]; then
  echo "::error::MODE must be 'drop' or 'truncate'"
  exit 1
fi

# Conditional to choose SQL template.
# String concatenation in SQL is done via || in PostgreSQL.
if [[ "$MODE" == "drop" ]]; then
  ACTION_TEMPLATE="SELECT 'DROP TABLE IF EXISTS \"' || schemaname || '\".\"' || tablename || '\" CASCADE;'"
else
  ACTION_TEMPLATE="SELECT 'TRUNCATE TABLE \"' || schemaname || '\".\"' || tablename || '\" RESTART IDENTITY CASCADE;'"
fi

# Build the generator query (not yet the DDL, but the SELECT that produces DDL).
FILTER="WHERE schemaname NOT IN ('pg_catalog','information_schema')"
KEEP_LIST="'__EFMigrationsHistory','schema_migrations','flyway_schema_history'"
SQL="$ACTION_TEMPLATE FROM pg_tables $FILTER AND tablename NOT IN ($KEEP_LIST);"

# psql -At -c:
#   -A : unaligned output (no padding).
#   -t : tuples only (suppress headers & footers).
# Redirect > planned.sql writes output to file (truncate/create).
echo "[clear_postgres] Generating statements..."
psql -At -c "$SQL" > planned.sql

# grep -c . file : counts lines that match '.' (any non-empty line).
# || true : ensures the pipeline doesn't fail script if grep returns non-zero.
COUNT=$(grep -c . planned.sql || true)
echo "[clear_postgres] Statements planned: $COUNT"
echo "---- Planned SQL ----"
cat planned.sql || true   # cat prints file contents; || true prevents failure if empty.
echo "---------------------"

# Numeric comparison -eq inside [[ ]].
if [[ "$COUNT" -eq 0 ]]; then
  echo "[clear_postgres] Nothing to do."
  exit 0
fi

# Dry run conditional; no execution when DRY_RUN == "true".
# psql -f file : executes SQL commands from a file.
if [[ "$DRY_RUN" == "true" ]]; then
  echo "[clear_postgres] Dry run mode; no changes applied."
else
  echo "[clear_postgres] Executing..."
  psql -v ON_ERROR_STOP=1 -f planned.sql
  echo "[clear_postgres] Execution complete."
fi

# Summary output, purely informational.
echo "[clear_postgres] Summary:"
echo " Mode:       $MODE"
echo " Dry run:    $DRY_RUN"
echo " SSL mode:   $PGSSLMODE"
echo " Statements: $COUNT"
if [[ "$VERIFY_SSL" != "true" ]]; then
  echo "::warning::Ran without SSL."
fi

echo "[clear_postgres] Done."
