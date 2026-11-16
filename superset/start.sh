#!/bin/bash
# Prepares Superset's own database and admin account on each start, then runs the web server.
set -e

superset db upgrade
superset fab create-admin \
  --username "$SUPERSET_ADMIN_USER" \
  --password "$SUPERSET_ADMIN_PASSWORD" \
  --firstname Expense --lastname Admin \
  --email admin@expense.local || true
superset init

exec gunicorn --bind 0.0.0.0:8088 --workers 2 --timeout 120 --limit-request-line 0 --limit-request-field_size 0 "superset.app:create_app()"
