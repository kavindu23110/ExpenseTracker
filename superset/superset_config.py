"""Superset settings for the Expense Tracker; secrets come from the environment."""
import os

SECRET_KEY = os.environ["SUPERSET_SECRET_KEY"]

# Superset's own settings (users, charts, dashboards) live in a file on a Docker volume.
SQLALCHEMY_DATABASE_URI = "sqlite:////app/superset_home/superset.db"

# Dashboards are shown inside the Statistics page using guest tokens from the Expense Tracker API.
FEATURE_FLAGS = {"EMBEDDED_SUPERSET": True}
GUEST_ROLE_NAME = "Gamma"
GUEST_TOKEN_JWT_SECRET = os.environ["SUPERSET_GUEST_TOKEN_SECRET"]
GUEST_TOKEN_JWT_EXP_SECONDS = 300

# Allow only the Expense Tracker site to frame Superset pages.
_app_origin = os.environ.get("EXPENSE_APP_ORIGIN", "http://172.30.0.50:3000")
TALISMAN_ENABLED = True
TALISMAN_CONFIG = {
    "content_security_policy": {
        "default-src": ["'self'"],
        "img-src": ["'self'", "blob:", "data:"],
        "worker-src": ["'self'", "blob:"],
        "connect-src": ["'self'"],
        "object-src": "'none'",
        "style-src": ["'self'", "'unsafe-inline'"],
        "script-src": ["'self'", "'strict-dynamic'"],
        "frame-ancestors": ["'self'", _app_origin],
    },
    "content_security_policy_nonce_in": ["script-src"],
    "force_https": False,
    "session_cookie_secure": False,
    "frame_options": None,
}
SESSION_COOKIE_SAMESITE = "Lax"

# The app reaches Superset over plain HTTP inside the private network.
ENABLE_PROXY_FIX = True
WTF_CSRF_ENABLED = True
