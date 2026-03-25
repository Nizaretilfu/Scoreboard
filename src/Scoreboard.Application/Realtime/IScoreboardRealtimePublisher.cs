namespace Scoreboard.Application.Realtime;

public interface IScoreboardRealtimePublisher
{
    Task PublishScoreRegisteredAsync(ScoreRegisteredRealtimeEvent @event, CancellationToken cancellationToken);
    Task PublishScoreCorrectedAsync(ScoreCorrectedRealtimeEvent @event, CancellationToken cancellationToken);
}
