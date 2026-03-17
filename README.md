# Competition Score System

System for managing competitions with live scoring.

## PR 1 backend foundation

This repository now includes a pragmatic backend skeleton for the MVP:

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

## Codex automation workflows

This repository includes GitHub Actions workflows that use the official Codex GitHub Action to support CI recovery and PR review remediation.

### Implemented workflows

- `.github/workflows/codex-on-ci-failure.yml`: triggers when the main `Build, Test and Coverage` workflow fails for a pull request.
- `.github/workflows/codex-on-pr-review.yml`: triggers when a PR gets a new review comment, or when a review is submitted with `changes_requested`.

### Required configuration

Add this repository secret before using the workflows:

- `OPENAI_API_KEY`: API key used by Codex GitHub Action.

The workflows use the default `GITHUB_TOKEN` with explicitly declared permissions for branch updates and PR automation.

### Prompt files

Codex task guidance is stored in:

- `.github/codex/prompts/fix-ci-failure.md`
- `.github/codex/prompts/address-pr-review.md`

Keep prompts focused and scoped so automation remains predictable and reviewable.
