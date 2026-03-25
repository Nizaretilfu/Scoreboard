using Microsoft.AspNetCore.SignalR;
using Scoreboard.Api.Hubs;
using Scoreboard.Application.Realtime;

namespace Scoreboard.Api.Realtime;

public sealed class SignalRScoreboardRealtimePublisher(IHubContext<LeaderboardHub> hubContext) : IScoreboardRealtimePublisher
{
    public async Task PublishScoreRegisteredAsync(ScoreRegisteredRealtimeEvent @event, CancellationToken cancellationToken)
    {
        var clients = hubContext.Clients.Group(LeaderboardHub.GetCompetitionGroupName(@event.CompetitionId));

        await clients.SendAsync(RealtimeEventNames.ScoreRegistered, @event, cancellationToken);
        await clients.SendAsync(RealtimeEventNames.LeaderboardUpdated, @event.Leaderboard, cancellationToken);

        if (@event.RankChanged is not null)
        {
            await clients.SendAsync(RealtimeEventNames.ParticipantRankChanged, @event.RankChanged, cancellationToken);
        }
    }

    public async Task PublishScoreCorrectedAsync(ScoreCorrectedRealtimeEvent @event, CancellationToken cancellationToken)
    {
        var clients = hubContext.Clients.Group(LeaderboardHub.GetCompetitionGroupName(@event.CompetitionId));

        await clients.SendAsync(RealtimeEventNames.ScoreCorrected, @event, cancellationToken);
        await clients.SendAsync(RealtimeEventNames.LeaderboardUpdated, @event.Leaderboard, cancellationToken);

        if (@event.RankChanged is not null)
        {
            await clients.SendAsync(RealtimeEventNames.ParticipantRankChanged, @event.RankChanged, cancellationToken);
        }
    }
}
