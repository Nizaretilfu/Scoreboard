# Competition Score System

System for managing competitions with live scoring.

## PR 1 backend foundation

This repository includes a pragmatic backend skeleton for the MVP:

- `Scoreboard.sln` with API, Application, Domain, Infrastructure, and test projects
- modular monolith folder boundaries for Competitions, Participants, Heats, Runs, Scoring, Leaderboard, and Shared
- EF Core `ScoreboardDbContext` with explicit mappings
- PostgreSQL wiring through `ScoreboardDatabase` connection string
- first migration creating competitions, participants, heats, runs, and score entries tables
- baseline unit tests for ring score invariants and EF model constraints

## Local development setup

### Prerequisites

- .NET 8 SDK
- PostgreSQL 15+ running locally

### 1) Create local database

By default (without `ASPNETCORE_ENVIRONMENT`), tooling and the API use `appsettings.json` (`scoreboard`).

If you want to use the local development database from `appsettings.Development.json`, set `ASPNETCORE_ENVIRONMENT=Development` when running EF commands and the API:

- Host: `localhost`
- Port: `5432`
- Database: `scoreboard_dev`
- Username: `postgres`
- Password: `postgres`

Create the database locally (example):

```bash
createdb -h localhost -p 5432 -U postgres scoreboard_dev
```

### 2) Apply migrations

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/Scoreboard.Infrastructure --startup-project src/Scoreboard.Api
```

### 3) Run the API

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Scoreboard.Api
```

Useful local endpoints (replace `<port>` with the port shown by `dotnet run`):

- API root: `http://localhost:<port>/`
- Health check: `http://localhost:<port>/health`
- Swagger UI (Development): `http://localhost:<port>/swagger`

### 4) Use Swagger + minimal API key auth

Scoring endpoints are protected by a lightweight API key auth scheme.

- Header: `X-Api-Key`
- Development key (default): `dev-scoreboard-key`
- Config path: `Authentication:ApiKey` in `appsettings*.json`

In Development, open Swagger UI and click **Authorize**, then paste the key value:

```
dev-scoreboard-key
```

Protected endpoints:

- `POST /api/scoring/scores` (register score for participant-in-run)
- `PUT /api/scoring/scores` (correct existing score)

### Leaderboard query + realtime updates

The MVP leaderboard is computed on demand from participant and score tables (no persisted projection).

Ranking rule is deterministic:

- highest total rings first
- if tied, lowest participant number

Endpoints and hubs:

- `GET /api/leaderboard/competitions/{competitionId}`
- SignalR hub: `/hubs/leaderboard`

SignalR semantic events for scoreboard clients:

- `scoreRegistered`
- `scoreCorrected`
- `leaderboardUpdated`
- `participantRankChanged`

Clients can call `JoinCompetition(competitionId)` to receive updates for one competition group.

Example curl commands:

```bash
curl -X POST "http://localhost:5000/api/scoring/scores" \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev-scoreboard-key" \
  -d '{"runId":"<run-id>","participantId":"<participant-id>","rings":2}'

curl -X PUT "http://localhost:5000/api/scoring/scores" \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev-scoreboard-key" \
  -d '{"runId":"<run-id>","participantId":"<participant-id>","rings":1}'
```

## Local automation helpers

To reduce repetitive command typing, use:

- `./scripts/dev.sh <command>`
- `make <target>`

Available commands/targets:

- `restore` -> restore NuGet packages
- `build` -> build solution in Release
- `test` -> run tests with coverage (`./TestResults`)
- `check` -> restore + build + test (CI-like local validation)
- `run-api` -> run API in Development environment
- `ef-update` -> apply migrations in Development environment

Examples:

```bash
./scripts/dev.sh check
make run-api
```

## CI/CD and collaboration workflow

### GitHub CI

The repository keeps GitHub Actions focused on standard validation only:

- `.github/workflows/build.yml`
  - Build + test on pull requests and pushes to `main`/`master`
  - Includes `workflow_dispatch` for manual validation

No API-key-based Codex automation is expected in GitHub Actions.

### Codex usage model

Codex support is handled outside GitHub Actions:

- Use Codex via Slack / Codex cloud
- For code review assistance, request it in PR discussion/review comments by mentioning `<@U0ALZ9Z1FT8>`
- Keep human review approval as the merge gate

This keeps CI deterministic (build/test only) while still enabling Codex-assisted workflows when explicitly requested.

## PR and issue templates

This repo includes:

- `.github/pull_request_template.md` for consistent, reviewable PRs
- `.github/ISSUE_TEMPLATE/bug_report.yml`
- `.github/ISSUE_TEMPLATE/automation_task.yml`

Use these templates to reduce manual triage and keep changes scoped and auditable.


## Frontend MVP app

A minimal React judge + live scoreboard UI is included in `src/Scoreboard.Frontend`.

It uses existing scoring and leaderboard APIs plus setup read endpoints:

- `GET /api/setup/competitions`
- `GET /api/setup/competitions/{competitionId}/runs`
- `POST /api/scoring/scores`
- `GET /api/leaderboard/competitions/{competitionId}`
- SignalR hub: `/hubs/leaderboard`

Run locally:

```bash
cd src/Scoreboard.Frontend
npm install
npm run dev
```

Run frontend tests:

```bash
cd src/Scoreboard.Frontend
npm test
```
