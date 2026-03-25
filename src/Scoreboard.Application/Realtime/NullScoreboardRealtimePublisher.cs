namespace Scoreboard.Application.Realtime;

public sealed class NullScoreboardRealtimePublisher : IScoreboardRealtimePublisher
{
    public Task PublishScoreRegisteredAsync(ScoreRegisteredRealtimeEvent @event, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task PublishScoreCorrectedAsync(ScoreCorrectedRealtimeEvent @event, CancellationToken cancellationToken) => Task.CompletedTask;
}
