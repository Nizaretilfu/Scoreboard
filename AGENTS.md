# AGENTS.md

## Project Overview

This project is a competition scoring system.

The MVP is designed for ring riding competitions, where a participant can score:
- 0 rings
- 1 ring
- 2 rings

The system must support:
- judge/notary score entry from mobile-friendly web UI
- management configuration of competitions, participants, heats and runs
- live scoreboard updates
- scoreboard animations when points or rankings change

Although the first version is focused on ring riding, the architecture should be extensible enough to support other sports and competition types later, such as football, handball and hockey.

Do not overengineer the first version.

The MVP must optimize for:
- fast score entry
- simple maintainable architecture
- high testability
- production-quality code

---

## Core Engineering Principles

- Prefer simple, maintainable solutions over unnecessary abstraction.
- Do not overengineer the MVP.
- Optimize the first version for ring riding, but keep the codebase extendable where reasonable.
- Favor clear architecture boundaries and readable code.
- Avoid speculative generic frameworks unless they clearly help the current MVP.
- Keep business logic out of controllers, hubs and UI components.
- Build for correctness, clarity, testability and reviewability.

---

## Required Tech Stack

### Backend
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- SignalR

### Frontend
- React web application

### Architecture Style
- modular monolith
- clean architecture light
- CQRS light
- event-driven realtime updates where appropriate

---

## Architecture Requirements

Use a modular monolith structure.

Preferred backend structure:
- feature-oriented / vertical-slice-friendly structure
- clear separation between domain, application, infrastructure and API concerns
- avoid large god-classes and avoid dumping everything into shared folders

Suggested backend modules:
- Competitions
- Participants
- Heats
- Runs
- Scoring
- Leaderboard
- Users/Auth
- Shared/Infrastructure

### Architecture Rules
- Controllers must stay thin.
- SignalR hubs must stay thin.
- Business logic belongs in application/domain layers.
- Persistence concerns belong in infrastructure.
- Domain rules must not depend on UI or transport details.
- EF Core entities and configuration should be explicit and clean.
- Use DTOs for API contracts where appropriate.
- Use commands and queries where it improves clarity.
- Prefer pragmatic CQRS-light, not heavy enterprise CQRS.

---

## Domain Guidance

The MVP domain is ring riding.

Important domain concepts include:
- Competition
- Participant
- Heat
- Run
- ScoreEntry
- Leaderboard

### Ring Riding MVP Rules
- Each participant is registered in advance with participant number and name.
- A competition can contain multiple heats.
- A heat can contain multiple runs.
- A run can contain multiple participants.
- For a participant in a run, a judge/notary must be able to quickly register 0, 1 or 2 rings.
- Judge/notary workflows must be optimized for speed and minimal friction.

### Future Flexibility
The system should be designed so that ring riding is the first supported discipline, not the last.

However:
- do not create an overly abstract generic sports engine in the first version
- only introduce abstractions when they serve the MVP or clearly support near-term extension
- prioritize a clean path for future extension over premature generalization

---

## Implementation Rules

When implementing a feature:
1. Plan first.
2. Explain the design briefly.
3. Implement the smallest clean solution that satisfies the requirement.
4. Add or update tests.
5. Ensure the solution builds and tests pass.

### Before coding
Always:
- read relevant repository instructions and project documents
- inspect existing code patterns
- check whether the requested change fits the current architecture
- prefer consistency with the repository style unless there is a strong reason to improve it

### When adding code
- prefer explicit and readable naming
- avoid hidden magic
- avoid deeply nested logic
- avoid unnecessary indirection
- avoid giant classes and giant files
- favor composition over tangled conditional logic
- keep methods focused and reasonably small
- prefer immutable request/response models where practical

### When changing code
- preserve existing behavior unless the task explicitly changes it
- update all affected tests
- avoid unrelated refactors unless necessary for correctness
- if a refactor is needed, keep it scoped and explain why

---

## Testing Requirements

Testing is mandatory.

### Unit Testing Rules
- All important business logic must be unit tested.
- New features must include relevant automated tests.
- Bug fixes should include regression tests when appropriate.
- Tests should focus on behavior, not implementation details.
- Prefer fast, deterministic unit tests.
- Keep domain and application logic highly testable.

### Test Design Guidance
- Test names should clearly describe behavior.
- Cover happy path, edge cases and important failure cases.
- Mock only where needed.
- Prefer testing real logic over over-mocking.
- Avoid brittle tests.
- Do not skip tests unless explicitly instructed.

### Minimum Expectation
Every meaningful backend feature should include:
- business logic tests
- validation tests where relevant
- regression coverage for fixed bugs

---

## Quality and Security Gates

Code quality and security are mandatory.

All pull requests must be suitable for:
- successful build
- passing automated unit tests
- code quality analysis
- security scanning
- human code review approval

Assume these checks are required before merge.

### Rules
- Do not propose bypassing quality gates.
- Do not suggest merging broken builds.
- Do not ignore failing tests.
- Do not remove tests to make builds pass.
- Do not weaken validation or security checks without explicit instruction.

---

## Pull Request Rules

- Never push directly to main or master.
- Always work through a branch and pull request.
- Keep PRs focused and reviewable.
- Include a clear summary of what changed.
- Mention important architectural decisions.
- Mention risks, assumptions and follow-up work when relevant.

### PR Summary Should Include
- what was implemented
- why it was implemented this way
- what tests were added or updated
- any known limitations or follow-ups

---

## Realtime and Scoreboard Guidance

Realtime updates are important.

### SignalR Guidance
- Use SignalR for live updates.
- Keep hub methods thin.
- Publish meaningful events from backend logic.
- Send semantic updates rather than leaking persistence details directly.

Examples of useful event concepts:
- ScoreRegistered
- LeaderboardUpdated
- ParticipantRankChanged

### Scoreboard Guidance
The scoreboard is an important UX surface.

It should support animations when:
- a participant receives points
- rankings change
- a participant moves up or down

Backend should expose clean realtime events that allow frontend animation without unnecessary coupling.

Do not put animation logic in the backend.

---

## EF Core and Data Access Guidance

- Use EF Core as the primary ORM.
- Use PostgreSQL as the target database.
- Use migrations.
- Keep entity mapping explicit.
- Use no-tracking queries where appropriate for read-only operations.
- Project to DTOs where it improves performance and clarity.
- Avoid loading large object graphs without need.
- Do not add repository abstractions unless they provide clear value.
- Prefer straightforward EF Core usage over unnecessary repository layers.

If a query becomes unusually complex or performance-sensitive, it is acceptable to use carefully scoped raw SQL or specialized query logic.

---

## API Guidance

- Design APIs around clear use cases.
- Keep request and response contracts explicit.
- Validate inputs properly.
- Return appropriate status codes.
- Keep controllers thin.
- Avoid leaking internal entity models directly to clients.

---

## Frontend Guidance

- Build a mobile-friendly web UI for judge/notary workflows.
- Optimize for speed and minimal input friction.
- Keep the judge flow extremely fast and simple.
- Prefer clear and responsive UI over overcomplicated visuals.
- Scoreboard UI may be more dynamic and animated, but should still remain maintainable.

---

## Documentation Rules

When adding new features or structural changes:
- update relevant documentation if needed
- keep README / architecture docs aligned with the implementation
- explain non-obvious decisions in code comments only where necessary
- prefer self-explanatory code over excessive comments

---

## Behavior Expectations for the Coding Agent

When given a task:
1. Read the relevant files first.
2. Plan before implementing.
3. Follow the existing architecture.
4. Keep the solution pragmatic.
5. Add tests.
6. Make sure the solution builds cleanly.

### If the request is ambiguous
- make the smallest reasonable assumption
- state the assumption clearly in the response
- avoid blocking progress unnecessarily

### If the request risks overengineering
- choose the simpler solution
- mention the simpler choice explicitly
- preserve a path for later extension

### If the requested change conflicts with these rules
- prefer maintainability, testability and MVP pragmatism
- call out the tradeoff clearly

---

## Absolute Do-Nots

- Do not overengineer the MVP.
- Do not introduce microservices.
- Do not introduce event sourcing.
- Do not introduce heavy generic sports abstractions too early.
- Do not push directly to main/master.
- Do not leave business logic untested.
- Do not bypass build, test, quality or security checks.
- Do not place core business logic in controllers, SignalR hubs or React components.
- Do not make large unrelated refactors without clear justification.

---

## Preferred Delivery Style

Prefer small, correct, reviewable increments.

Build the system in a way that is:
- testable
- maintainable
- secure
- pragmatic
- easy to review
- ready for future extension without unnecessary complexity

## Chief Architect Operating Mode

When asked to act as Chief Architect, Codex must:

1. Read PROJECT.md, ARCHITECTURE.md and AGENTS.md first.
2. Clarify the requested feature in terms of domain, architecture and risks.
3. Propose a short implementation plan before writing code.
4. Split work into small, reviewable increments.
5. Prefer one focused pull request per logical change.
6. Ensure all important business logic is covered by unit tests.
7. Keep architecture pragmatic and avoid overengineering.
8. Preserve modular monolith boundaries.
9. Call out assumptions explicitly.
10. Summarize:
   - what to build now
   - what to postpone
   - what technical debt is intentionally accepted

When appropriate, the Chief Architect should propose subtasks such as:
- backend/domain modeling
- API design
- realtime events
- frontend UI
- test coverage
- CI / quality improvements

Do not implement everything at once unless explicitly requested.
Prefer staged delivery.

## CI policy

If a pull request fails CI:

1. Inspect GitHub Actions logs
2. Identify compile or test failures
3. Push a fix commit
4. Repeat until CI passes
