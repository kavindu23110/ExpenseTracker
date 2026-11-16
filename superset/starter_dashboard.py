"""Creates a starter "Household spending" dashboard and enables embedding; prints the embed ID."""
import json
import os

import requests

BASE = "http://localhost:8088"
APP_ORIGIN = os.environ.get("EXPENSE_APP_ORIGIN", "http://172.30.0.50:3000")
s = requests.Session()
token = s.post(f"{BASE}/api/v1/security/login", json={"username": os.environ["SUPERSET_ADMIN_USER"], "password": os.environ["SUPERSET_ADMIN_PASSWORD"], "provider": "db", "refresh": False}).json()["access_token"]
s.headers["Authorization"] = f"Bearer {token}"
s.headers["X-CSRFToken"] = s.get(f"{BASE}/api/v1/security/csrf_token/").json()["result"]
s.headers["Referer"] = BASE


def find(kind, col, value):
    q = f"(filters:!((col:{col},opr:eq,value:'{value}')))"
    result = s.get(f"{BASE}/api/v1/{kind}/", params={"q": q}).json()["result"]
    return result[0]["id"] if result else None


lines = find("dataset", "table_name", "ReportBillLines")
TITLE = "Household spending"
dash = find("dashboard", "dashboard_title", TITLE)
if dash is None:
    dash = s.post(f"{BASE}/api/v1/dashboard/", json={"dashboard_title": TITLE, "published": True}).json()["id"]

SUM = {"expressionType": "SIMPLE", "column": {"column_name": "Amount"}, "aggregate": "SUM", "label": "Spent (Rs)"}
charts = [
    ("Spending by category", "pie", {"groupby": ["Category"], "metric": SUM, "row_limit": 50, "show_legend": True, "label_type": "key_percent"}),
    ("Spending by month and category", "echarts_timeseries_bar", {"x_axis": "BillDate", "time_grain_sqla": "P1M", "metrics": [SUM], "groupby": ["Category"], "stack": "Stack", "row_limit": 10000, "show_legend": True}),
    ("Top shops", "echarts_timeseries_bar", {"x_axis": "Shop", "metrics": [SUM], "groupby": [], "orientation": "horizontal", "row_limit": 15, "x_axis_sort": "Spent (Rs)", "x_axis_sort_asc": False}),
    ("Spending by who paid", "pie", {"groupby": ["PaidBy"], "metric": SUM, "row_limit": 20, "show_legend": True, "label_type": "key_value"}),
]
for name, viz, params in charts:
    if find("chart", "slice_name", name):
        continue
    form = {"viz_type": viz, "datasource": f"{lines}__table", "adhoc_filters": [], **params}
    r = s.post(f"{BASE}/api/v1/chart/", json={"slice_name": name, "viz_type": viz, "datasource_id": lines, "datasource_type": "table", "params": json.dumps(form), "dashboards": [dash]})
    print("chart", name, r.status_code, r.text[:200] if r.status_code >= 300 else "")

r = s.post(f"{BASE}/api/v1/dashboard/{dash}/embedded", json={"allowed_domains": [APP_ORIGIN]})
print("EMBED_ID", r.json()["result"]["uuid"])
