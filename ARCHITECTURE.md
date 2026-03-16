# Architecture

This system uses a modular monolith architecture.

Backend:
ASP.NET Core Web API

Database:
PostgreSQL using EF Core

Realtime:
SignalR

Frontend:
React web application

Modules:
- Competitions
- Participants
- Scoring
- Leaderboard
- Users

Events:
- ScoreRegistered
- LeaderboardUpdated
- ParticipantRankChanged

Principles:
- Clean architecture (light)
- CQRS-light
- Feature-based folders
