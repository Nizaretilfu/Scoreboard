# Competition Scoring System

## Purpose

The system is initially intended for ring riding competitions.

The goal is to support fast and reliable live score registration during a competition, while also allowing management to configure competitions, participants, heats and runs, and to show a live scoreboard.

Although the first version is built for ring riding, the system should be designed in a generic and extensible way so it can later support other sports and competition types such as football, handball, hockey and similar disciplines.

## MVP Domain: Ring Riding

In ring riding, each participant rides in a run and attempts to hit up to 2 rings.

For each participant in a run, a judge/notary must be able to quickly register whether the participant got:

- 0 rings
- 1 ring
- 2 rings

The user experience for judges/notaries must be extremely fast and simple.

All participants are registered in advance with:

- participant number
- participant name

## Competition Structure

A competition can contain:

- multiple heats
- multiple runs within a heat
- multiple participants within a run

## Core Requirements

### Judge / Notary View

The system must make it fast and easy to:

- select participant number
- see participant name
- register score for the current run
- choose between 0, 1 or 2 rings
- move quickly to the next participant

The scoring UI must be optimized for speed and minimal input friction.

### Management View

Management must be able to:

- create and configure competitions
- register participants
- define heats
- define runs within heats
- assign participants to runs
- monitor scores live
- correct scores if needed

### Scoreboard View

The system must provide a live scoreboard that updates in real time when scores are registered.

The scoreboard should support animations when:
- a participant receives points
- rankings change
- a participant moves up or down in the standings

## Long-Term Goal: Generic Competition Engine

The system should be designed so that ring riding is the first supported discipline, but not the only one.

The architecture should allow future support for other sports and competition types by introducing configurable competition formats, scoring models and presentation rules.

Examples of future disciplines:
- football
- handball
- hockey

This means the architecture should avoid hardcoding too much ring-riding-specific logic into the core platform.
Instead, ring riding should be implemented as the first concrete scoring mode on top of a more generic competition model where reasonable.

## Technical Direction

Backend:
- ASP.NET Core
- EF Core
- PostgreSQL
- SignalR

Frontend:
- React web application

Architecture:
- modular monolith
- clean architecture light
- CQRS light
- event-driven realtime updates

## Important Design Principles

- Do not overengineer the first version.
- The MVP must prioritize ring riding and make that workflow excellent.
- The system should be extensible, but pragmatism is more important than premature abstraction.
- Prefer simple, maintainable solutions over complex generic frameworks in the first version.

## Quality Requirements

Code quality is a first-class requirement.

The codebase must follow modern engineering best practices with a strong focus on:

- readability
- maintainability
- testability
- security
- clear architecture boundaries

All important business logic must be covered by automated unit tests.

## Testing Requirements

- All core business logic must be unit tested.
- New features must include relevant unit tests.
- Bug fixes should include regression tests where appropriate.
- Pull requests must not be merged if tests are failing.
- The solution should be designed to support high testability from the start.

## CI/CD and Merge Requirements

Before any code can be merged into the main branch, all of the following must pass:

- build validation
- automated unit tests
- code quality scans
- security scans
- required code review approval

No direct pushes to the main/master branch are allowed.

All changes must go through pull requests.

## Security and Quality Gates

The project should include automated checks for:

- static code quality analysis
- dependency vulnerability scanning
- secure coding issues where possible

These checks are mandatory and must pass before merge approval.

## Important Delivery Principle

The system should be built in a way that supports fast iteration, but without compromising quality.

The goal is not just to generate code quickly, but to generate code that is production-quality, testable, secure and reviewable.
