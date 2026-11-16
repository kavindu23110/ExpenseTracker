"""Adds the Expense Tracker database and its report views to Superset; safe to run again."""
import os
import urllib.parse

import requests

BASE = "http://localhost:8088"
s = requests.Session()
token = s.post(f"{BASE}/api/v1/security/login", json={"username": os.environ["SUPERSET_ADMIN_USER"], "password": os.environ["SUPERSET_ADMIN_PASSWORD"], "provider": "db", "refresh": False}).json()["access_token"]
s.headers["Authorization"] = f"Bearer {token}"
s.headers["X-CSRFToken"] = s.get(f"{BASE}/api/v1/security/csrf_token/").json()["result"]
s.headers["Referer"] = BASE

NAME = "Expense Tracker"
password = urllib.parse.quote_plus(os.environ["SUPERSET_DB_PASSWORD"])
uri = f"mssql+pymssql://superset_reader:{password}@host.docker.internal:1433/ExpenseTracker"

existing = s.get(f"{BASE}/api/v1/database/", params={"q": "(filters:!((col:database_name,opr:eq,value:'Expense Tracker')))"}).json()["result"]
if existing:
    db_id = existing[0]["id"]
    r = s.put(f"{BASE}/api/v1/database/{db_id}", json={"sqlalchemy_uri": uri})
else:
    r = s.post(f"{BASE}/api/v1/database/", json={"database_name": NAME, "sqlalchemy_uri": uri, "expose_in_sqllab": True, "allow_dml": False})
    db_id = r.json().get("id")
print("database", r.status_code, db_id)

for view in ["ReportBillLines", "ReportBills"]:
    q = f"(filters:!((col:table_name,opr:eq,value:'{view}')))"
    if s.get(f"{BASE}/api/v1/dataset/", params={"q": q}).json()["result"]:
        print("dataset", view, "exists")
        continue
    r = s.post(f"{BASE}/api/v1/dataset/", json={"database": db_id, "schema": "dbo", "table_name": view})
    print("dataset", view, r.status_code, r.text[:200] if r.status_code >= 300 else "")
