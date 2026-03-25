using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scoreboard.Api.Contracts;
using Scoreboard.Application.CompetitionSetup;
using Scoreboard.Application.Scoring;

namespace Scoreboard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/scoring")]
public sealed class ScoringController(ScoringService scoringService) : ControllerBase
{
    [HttpPost("scores")]
    public async Task<IResult> RegisterScore([FromBody] RegisterScoreApiRequest request, CancellationToken cancellationToken)
    {
        var result = await scoringService.RegisterScoreAsync(
            new RegisterScoreRequest(request.RunId, request.ParticipantId, request.Rings),
            cancellationToken);

        return ToResult(result, StatusCodes.Status201Created);
    }

    [HttpPut("scores")]
    public async Task<IResult> CorrectScore([FromBody] CorrectScoreApiRequest request, CancellationToken cancellationToken)
    {
        var result = await scoringService.CorrectScoreAsync(
            new CorrectScoreRequest(request.RunId, request.ParticipantId, request.Rings),
            cancellationToken);

        return ToResult(result, StatusCodes.Status200OK);
    }

    private static IResult ToResult<T>(OperationResult<T> result, int successStatusCode)
    {
        if (result.IsSuccess)
        {
            return Results.Json(result.Value, statusCode: successStatusCode);
        }

        var statusCode = result.Error!.Code.EndsWith("not_found", StringComparison.Ordinal)
            ? StatusCodes.Status404NotFound
            : result.Error.Code.EndsWith("already_registered", StringComparison.Ordinal)
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

        return Results.Json(result.Error, statusCode: statusCode);
    }
}
