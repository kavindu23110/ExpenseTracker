# Expense Tracker

Scan bills in English or Sinhala, file them, and compare what you paid with market prices and inflation.

| Part | Stack | Runs on |
| --- | --- | --- |
| `frontend/` | Next.js, React Query, Tailwind | http://localhost:3200 |
| `backend/` | ASP.NET Core 10, EF Core | http://localhost:5200 |
| Database | SQL Server, `ExpenseTracker` database | localhost |

## Run it

```
cd backend
dotnet run --project ExpenseTracker.Api --launch-profile http

cd frontend
npm install
npm run dev
```

In Development the API creates the database and seeds the price reference data. It starts with no bills.

## Read real bills

Uploads are read through the Claude Code CLI signed in on this machine (`Claude:UseCli` in `appsettings.json`), using your Claude plan's usage. This only works where Claude Code is installed and signed in, under the same Windows user as the API. If the CLI is not found, the app still runs and uploads show a clear message.

To use an API key instead, set `Claude:UseCli` to `false` and run:

```
cd backend/ExpenseTracker.Api
dotnet user-secrets set "Claude:ApiKey" "<your key>"
```

## Host with Docker

Hosted the same way as DDoc: private, with no host ports, and reached only through Cloudflare WARP. `compose.yaml` runs the API and the frontend, using the SQL Server installed on this machine. The frontend has a fixed address on DDoc's Docker network, `ddoc-hosted_ddoc-hosted`, so DDoc's tunnel carries the traffic, and DDoc must have created that network first.

**Open it at http://172.30.0.50:3000 with WARP connected.** Devices are set up as in DDoc's `HOW-TO.txt`.

1. Create a SQL login named `expense_app` with access to the `ExpenseTracker` database, and put its password in `.env` next to `compose.yaml` as `SQL_PASSWORD`. SQL Server must accept TCP connections on port 1433.
2. Run `docker compose up -d --build`.
3. Sign Claude Code in, once: `docker compose exec -it api claude`, then `/login`.
4. Create the owner account straight away, because the first visitor becomes the owner.

To deploy code changes later, run `redeploy.cmd`.

Bills, including their images, live in the local `ExpenseTracker` database, so back that up. The Claude sign-in is kept in a Docker volume. If this PC is off or asleep, or Docker isn't running, nobody can reach the site.

## Household features

The first visit creates the owner account; the owner adds members under Household. Everyone shares the same data. Tabs cover budgets, recurring bills, savings goals, your own price history, pantry and shopping list, and pasted bank SMS.

## Restart

Run `.estart.ps1` or double-click `restart.cmd`. It rebuilds the API and restarts both servers, with logs in `.logs/`. It will not stop other programs using ports 5200 or 3200.

## Layout

- `ExpenseTracker.Domain`: bills, categorising and price rules. All business rules live here.
- `ExpenseTracker.Application`: services and ports.
- `ExpenseTracker.Infrastructure`: EF Core, seeds and the `ClaudeBillReaderAdapter`.
- `ExpenseTracker.Api`: HTTP endpoints only.

Limits are in `appsettings.json` under `Bills`. Inflation, market prices and timelines are rows in the database and do not update themselves.
