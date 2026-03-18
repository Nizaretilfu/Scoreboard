## Summary
- What changed?
- Why was this approach chosen?

## Scope Check
- [ ] No application/domain behavior changes
- [ ] Changes are focused on automation/devex only
- [ ] Security boundaries for secrets/forks were preserved

## Automation / CI Impact
- [ ] Workflow changes were kept least-privilege
- [ ] Trusted-actor constraints reviewed (if applicable)
- [ ] Any new repo secrets/variables documented in `README.md`

## Testing / Validation
- [ ] `dotnet restore Scoreboard.sln`
- [ ] `dotnet build Scoreboard.sln --configuration Release`
- [ ] `dotnet test Scoreboard.sln --configuration Release`

## Risks & Follow-ups
- Known risks:
- Optional follow-up tasks:
