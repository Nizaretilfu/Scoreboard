using Microsoft.EntityFrameworkCore;
using Scoreboard.Application.Abstractions;

namespace Scoreboard.Application.Leaderboard;

public sealed class LeaderboardQueryService(IScoreboardDbContext dbContext)
{
    public async Task<CompetitionLeaderboardDto> GetCompetitionLeaderboardAsync(
        GetCompetitionLeaderboardRequest request,
        CancellationToken cancellationToken)
    {
        var totalsByParticipant = await (
                from score in dbContext.ScoreEntries.AsNoTracking()
                join run in dbContext.Runs.AsNoTracking() on score.RunId equals run.Id
                join heat in dbContext.Heats.AsNoTracking() on run.HeatId equals heat.Id
                where heat.CompetitionId == request.CompetitionId
                group score by score.ParticipantId
                into grouped
                select new
                {
                    ParticipantId = grouped.Key,
                    TotalRings = grouped.Sum(x => x.Rings)
                })
            .ToDictionaryAsync(x => x.ParticipantId, x => x.TotalRings, cancellationToken);

        var participants = await dbContext.Participants
            .AsNoTracking()
            .Where(p => p.CompetitionId == request.CompetitionId)
            .Select(p => new
            {
                p.Id,
                p.Number,
                p.Name
            })
            .ToListAsync(cancellationToken);

        var rows = participants
            .Select(participant => new
            {
                participant.Id,
                participant.Number,
                participant.Name,
                TotalRings = totalsByParticipant.GetValueOrDefault(participant.Id, 0)
            })
            .OrderByDescending(x => x.TotalRings)
            .ThenBy(x => x.Number)
            .Select((x, index) => new LeaderboardRowDto(
                index + 1,
                x.Id,
                x.Number,
                x.Name,
                x.TotalRings))
            .ToList();

        return new CompetitionLeaderboardDto(request.CompetitionId, rows, DateTimeOffset.UtcNow);
    }
}
