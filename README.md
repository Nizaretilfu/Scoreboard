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

## CI/CD and Codex automation

### Workflows

- `.github/workflows/build.yml`
  - Build + test on PRs and pushes to `main`/`master`
  - includes `workflow_dispatch` for manual verification
  - uses NuGet cache and concurrency cancellation for faster feedback
- `.github/workflows/codex-on-ci-failure.yml`
  - runs only after failed PR CI from same repository and trusted actors
  - applies minimal fix commits and re-triggers checks until build/test/coverage are green
  - requires trusted actor + API key
- `.github/workflows/codex-on-pr-review.yml`
  - responds to review comments / `changes_requested`
  - applies scoped fixes, reruns checks, and repeats on follow-up feedback until no blocking review comments remain
  - restricted to same-repo PRs, trusted actors, and configured API key

### Required repository configuration

| Type | Name | Required | Purpose |
|---|---|---|---|
| Secret | `OPENAI_API_KEY` | Yes (for Codex workflows) | Auth for Codex GitHub Action |
| Variable | `CODEX_TRUSTED_ACTORS` | Yes (for Codex workflows) | Comma-separated allowlist of users who can trigger write-capable automation |

Template values for trusted actors are provided in:

- `.github/codex/trusted-actors.example.txt`

### Security guardrails

- Automation that can write to branches does **not** run on fork PRs.
- Trusted-actor checks are enforced before Codex remediation workflows execute.
- Set `CODEX_TRUSTED_ACTORS` to a non-empty comma-separated allowlist (for example: `alice,bob`); when it is unset or empty, Codex workflows are skipped by design.
- Workflow permissions are explicitly declared and scoped.
- If required secrets/variables are missing, automation safely skips execution.

## PR and issue templates

This repo includes:

- `.github/pull_request_template.md` for consistent, reviewable PRs
- `.github/ISSUE_TEMPLATE/bug_report.yml`
- `.github/ISSUE_TEMPLATE/automation_task.yml`

Use these templates to reduce manual triage and keep changes scoped and auditable.

## Codex prompt files

Codex task guidance is stored in:

- `.github/codex/prompts/fix-ci-failure.md`
- `.github/codex/prompts/address-pr-review.md`

Keep prompts focused and scoped so automation remains predictable and reviewable.
