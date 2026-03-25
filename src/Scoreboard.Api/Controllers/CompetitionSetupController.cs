using Microsoft.AspNetCore.Mvc;
using Scoreboard.Api.Contracts;
using Scoreboard.Application.CompetitionSetup;

namespace Scoreboard.Api.Controllers;

[ApiController]
[Route("api/setup")]
public sealed class CompetitionSetupController(CompetitionSetupService setupService) : ControllerBase
{

    [HttpGet("competitions")]
    public async Task<IReadOnlyList<CompetitionOverviewApiResponse>> GetCompetitions(CancellationToken cancellationToken)
    {
        var competitions = await setupService.GetCompetitionsAsync(cancellationToken);
        return competitions
            .Select(x => new CompetitionOverviewApiResponse(x.Id, x.Name, x.CompetitionDate))
            .ToList();
    }

    [HttpGet("competitions/{competitionId:guid}/runs")]
    public async Task<IReadOnlyList<CompetitionRunOverviewApiResponse>> GetCompetitionRuns(Guid competitionId, CancellationToken cancellationToken)
    {
        var runs = await setupService.GetCompetitionRunsAsync(competitionId, cancellationToken);
        return runs
            .Select(run => new CompetitionRunOverviewApiResponse(
                run.RunId,
                run.HeatId,
                run.HeatSequenceNumber,
                run.RunSequenceNumber,
                run.Participants
                    .Select(p => new RunParticipantOverviewApiResponse(p.ParticipantId, p.ParticipantNumber, p.ParticipantName))
                    .ToList()))
            .ToList();
    }

    [HttpPost("competitions")]
    public async Task<IResult> CreateCompetition([FromBody] CreateCompetitionApiRequest request, CancellationToken cancellationToken)
    {
        var result = await setupService.CreateCompetitionAsync(new CreateCompetitionRequest(request.Name, request.CompetitionDate), cancellationToken);
        return ToResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("participants")]
    public async Task<IResult> RegisterParticipant([FromBody] RegisterParticipantApiRequest request, CancellationToken cancellationToken)
    {
        var result = await setupService.RegisterParticipantAsync(new RegisterParticipantRequest(request.CompetitionId, request.Number, request.Name), cancellationToken);
        return ToResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("heats")]
    public async Task<IResult> CreateHeat([FromBody] CreateHeatApiRequest request, CancellationToken cancellationToken)
    {
        var result = await setupService.CreateHeatAsync(new CreateHeatRequest(request.CompetitionId, request.SequenceNumber), cancellationToken);
        return ToResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("runs")]
    public async Task<IResult> CreateRun([FromBody] CreateRunApiRequest request, CancellationToken cancellationToken)
    {
        var result = await setupService.CreateRunAsync(new CreateRunRequest(request.HeatId, request.SequenceNumber), cancellationToken);
        return ToResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("run-assignments")]
    public async Task<IResult> AssignParticipantToRun([FromBody] AssignParticipantToRunApiRequest request, CancellationToken cancellationToken)
    {
        var result = await setupService.AssignParticipantToRunAsync(new AssignParticipantToRunRequest(request.RunId, request.ParticipantId), cancellationToken);
        return ToResult(result, StatusCodes.Status201Created);
    }

    private static IResult ToResult<T>(OperationResult<T> result, int successStatusCode)
    {
        if (result.IsSuccess)
        {
            return Results.Json(result.Value, statusCode: successStatusCode);
        }

        var statusCode = result.Error!.Code.EndsWith("not_found", StringComparison.Ordinal)
            ? StatusCodes.Status404NotFound
            : result.Error.Code.EndsWith("conflict", StringComparison.Ordinal)
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

        return Results.Json(result.Error, statusCode: statusCode);
    }
}
