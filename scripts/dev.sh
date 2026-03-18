#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

cmd="${1:-help}"

case "$cmd" in
  restore)
    dotnet restore Scoreboard.sln
    ;;
  build)
    dotnet build Scoreboard.sln --configuration Release
    ;;
  test)
    dotnet test Scoreboard.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults
    ;;
  check)
    dotnet restore Scoreboard.sln
    dotnet build Scoreboard.sln --no-restore --configuration Release
    dotnet test Scoreboard.sln --no-build --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults
    ;;
  run-api)
    ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Scoreboard.Api
    ;;
  ef-update)
    ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/Scoreboard.Infrastructure --startup-project src/Scoreboard.Api
    ;;
  *)
    cat <<'USAGE'
Usage: ./scripts/dev.sh <command>

Commands:
  restore    Restore NuGet packages
  build      Build solution in Release
  test       Run tests with coverage output in ./TestResults
  check      Restore + build + test (CI-like local validation)
  run-api    Run API in Development
  ef-update  Apply EF Core migrations using Development environment
USAGE
    ;;
esac
