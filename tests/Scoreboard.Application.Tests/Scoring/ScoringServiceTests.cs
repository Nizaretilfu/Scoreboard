using Microsoft.EntityFrameworkCore;
using Scoreboard.Application.Leaderboard;
using Scoreboard.Application.Realtime;
using Scoreboard.Application.Scoring;
using Scoreboard.Domain.Competitions;
using Scoreboard.Domain.Heats;
using Scoreboard.Domain.Participants;
using Scoreboard.Domain.RunParticipants;
using Scoreboard.Domain.Runs;
using Scoreboard.Infrastructure.Persistence;
using Xunit;

namespace Scoreboard.Application.Tests.Scoring;

public sealed class ScoringServiceTests
{
    [Fact]
    public async Task RegisterScore_ReturnsNotFound_WhenParticipantIsNotAssignedToRun()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.RegisterScoreAsync(new RegisterScoreRequest(Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("run_participant_not_found", result.Error?.Code);
    }

    [Fact]
    public async Task RegisterScore_ReturnsConflict_WhenScoreAlreadyExists()
    {
        await using var context = CreateContext();
        var (runId, participantId) = await SeedRunParticipantAsync(context);

        var service = CreateService(context);
        var first = await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 1), CancellationToken.None);
        Assert.True(first.IsSuccess);

        var second = await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 2), CancellationToken.None);

        Assert.False(second.IsSuccess);
        Assert.Equal("score_already_registered", second.Error?.Code);
    }

    [Fact]
    public async Task RegisterScore_PublishesSemanticRealtimeEvent_WithRankChange()
    {
        await using var context = CreateContext();

        var competition = new Competition(Guid.NewGuid(), "Cup", new DateOnly(2026, 3, 20));
        var heat = new Heat(Guid.NewGuid(), competition.Id, 1);
        var run = new Run(Guid.NewGuid(), heat.Id, 1);
        var participantA = new Participant(Guid.NewGuid(), competition.Id, 10, "A");
        var participantB = new Participant(Guid.NewGuid(), competition.Id, 11, "B");

        context.AddRange(
            competition,
            heat,
            run,
            participantA,
            participantB,
            new RunParticipant(Guid.NewGuid(), run.Id, participantA.Id),
            new RunParticipant(Guid.NewGuid(), run.Id, participantB.Id));

        await context.SaveChangesAsync();

        var publisher = new RecordingRealtimePublisher();
        var service = CreateService(context, publisher);

        var result = await service.RegisterScoreAsync(new RegisterScoreRequest(run.Id, participantB.Id, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(publisher.ScoreRegisteredEvents);

        var @event = publisher.ScoreRegisteredEvents.Single();
        Assert.Equal(competition.Id, @event.CompetitionId);
        Assert.Equal(participantB.Id, @event.ParticipantId);
        Assert.NotNull(@event.RankChanged);
        Assert.Equal(2, @event.RankChanged!.PreviousRank);
        Assert.Equal(1, @event.RankChanged.NewRank);
    }

    [Fact]
    public async Task CorrectScore_UpdatesRings_WhenScoreExists()
    {
        await using var context = CreateContext();
        var (runId, participantId) = await SeedRunParticipantAsync(context);

        var service = CreateService(context);
        await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 0), CancellationToken.None);

        var correction = await service.CorrectScoreAsync(new CorrectScoreRequest(runId, participantId, 2), CancellationToken.None);

        Assert.True(correction.IsSuccess);
        Assert.Equal(2, correction.Value?.Rings);
    }

    [Fact]
    public async Task CorrectScore_ReturnsNotFound_WhenScoreDoesNotExist()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.CorrectScoreAsync(new CorrectScoreRequest(Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("score_not_found", result.Error?.Code);
    }

    private static ScoringService CreateService(ScoreboardDbContext context, RecordingRealtimePublisher? publisher = null)
    {
        return new ScoringService(
            context,
            new LeaderboardQueryService(context),
            publisher ?? new RecordingRealtimePublisher());
    }

    private static ScoreboardDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ScoreboardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ScoreboardDbContext(options);
    }

    private static async Task<(Guid RunId, Guid ParticipantId)> SeedRunParticipantAsync(ScoreboardDbContext context)
    {
        var competition = new Competition(Guid.NewGuid(), "Cup", new DateOnly(2026, 3, 20));
        var heat = new Heat(Guid.NewGuid(), competition.Id, 1);
        var run = new Run(Guid.NewGuid(), heat.Id, 1);
        var participant = new Participant(Guid.NewGuid(), competition.Id, 12, "Rider");
        var assignment = new RunParticipant(Guid.NewGuid(), run.Id, participant.Id);

        context.AddRange(competition, heat, run, participant, assignment);
        await context.SaveChangesAsync();

        return (run.Id, participant.Id);
    }

    private sealed class RecordingRealtimePublisher : IScoreboardRealtimePublisher
    {
        public List<ScoreRegisteredRealtimeEvent> ScoreRegisteredEvents { get; } = new();
        public List<ScoreCorrectedRealtimeEvent> ScoreCorrectedEvents { get; } = new();

        public Task PublishScoreRegisteredAsync(ScoreRegisteredRealtimeEvent @event, CancellationToken cancellationToken)
        {
            ScoreRegisteredEvents.Add(@event);
            return Task.CompletedTask;
        }

        public Task PublishScoreCorrectedAsync(ScoreCorrectedRealtimeEvent @event, CancellationToken cancellationToken)
        {
            ScoreCorrectedEvents.Add(@event);
            return Task.CompletedTask;
        }
    }
}
