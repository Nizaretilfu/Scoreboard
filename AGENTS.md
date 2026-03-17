# Coding Agent Instructions

The project uses:

Backend:
ASP.NET Core
Entity Framework Core
PostgreSQL
SignalR

Frontend:
React

Architecture:
Modular monolith.

Guidelines:
- Always create Pull Requests
- Do not push directly to main
- Use feature folders
- Write clear code
- Prefer simple solutions
- Follow clean architecture principles

When implementing features:
1. Create domain models
2. Add EF configuration
3. Implement commands/queries
4. Add API endpoints
5. Add SignalR events if needed

## Mandatory Engineering Rules

- Always add or update unit tests for relevant business logic.
- Do not create pull requests with failing tests.
- Prefer testable designs with clear separation of concerns.
- Do not push directly to main or master.
- All work must go through pull requests.
- Assume code quality checks and security scans are mandatory.
- Prefer simple, maintainable solutions over unnecessary abstraction.
- Do not overengineer the first version.
