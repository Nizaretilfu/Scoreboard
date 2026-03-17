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

## Running migrations

```bash
dotnet ef database update --project src/Scoreboard.Infrastructure --startup-project src/Scoreboard.Api
```
