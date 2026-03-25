using Microsoft.AspNetCore.Mvc;
using Scoreboard.Api.Contracts;
using Scoreboard.Application.Leaderboard;

namespace Scoreboard.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public sealed class LeaderboardController(LeaderboardQueryService leaderboardQueryService) : ControllerBase
{
    [HttpGet("competitions/{competitionId:guid}")]
    public async Task<CompetitionLeaderboardApiResponse> GetCompetitionLeaderboard(Guid competitionId, CancellationToken cancellationToken)
    {
        var result = await leaderboardQueryService.GetCompetitionLeaderboardAsync(
            new GetCompetitionLeaderboardRequest(competitionId),
            cancellationToken);

        return new CompetitionLeaderboardApiResponse(
            result.CompetitionId,
            result.Rows.Select(x => new LeaderboardRowApiResponse(
                    x.Rank,
                    x.ParticipantId,
                    x.ParticipantNumber,
                    x.ParticipantName,
                    x.TotalRings))
                .ToList(),
            result.GeneratedAtUtc);
    }
}
