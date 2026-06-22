using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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
    public async Task RegisterScore_ReturnsExistingScore_WhenClientSubmissionIsRetried()
    {
        await using var context = CreateContext();
        var (runId, participantId) = await SeedRunParticipantAsync(context);
        var clientSubmissionId = Guid.NewGuid();
        var publisher = new RecordingRealtimePublisher();
        var service = CreateService(context, publisher);

        var first = await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 2, clientSubmissionId), CancellationToken.None);
        var retry = await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 2, clientSubmissionId), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(first.Value?.Id, retry.Value?.Id);
        Assert.Equal(clientSubmissionId, retry.Value?.ClientSubmissionId);
        Assert.Single(context.ScoreEntries);
        Assert.Single(publisher.ScoreRegisteredEvents);
    }

    [Fact]
    public async Task RegisterScore_ReturnsConflict_WhenClientSubmissionIdIsReusedForDifferentScore()
    {
        await using var context = CreateContext();
        var (runId, participantId) = await SeedRunParticipantAsync(context);
        var clientSubmissionId = Guid.NewGuid();
        var service = CreateService(context);

        await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 1, clientSubmissionId), CancellationToken.None);
        var retry = await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 2, clientSubmissionId), CancellationToken.None);

        Assert.False(retry.IsSuccess);
        Assert.Equal("client_submission_id_conflict", retry.Error?.Code);
    }

    [Fact]
    public async Task RegisterScore_AllowsSameParticipantInMultipleRunsWithinSameHeat_WhenSubmissionIdsAreDifferent()
    {
        await using var context = CreateContext();
        var (firstRunId, secondRunId, participantId) = await SeedParticipantInTwoRunsWithinSameHeatAsync(context);
        var service = CreateService(context);

        var firstRunScore = await service.RegisterScoreAsync(
            new RegisterScoreRequest(firstRunId, participantId, 2, Guid.NewGuid()),
            CancellationToken.None);
        var secondRunScore = await service.RegisterScoreAsync(
            new RegisterScoreRequest(secondRunId, participantId, 1, Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(firstRunScore.IsSuccess);
        Assert.True(secondRunScore.IsSuccess);
        Assert.Equal(firstRunId, firstRunScore.Value?.RunId);
        Assert.Equal(secondRunId, secondRunScore.Value?.RunId);
        Assert.Equal(2, context.ScoreEntries.Count(x => x.ParticipantId == participantId));
    }

    [Fact]
    public async Task RegisterScore_RetryForEarlierRunDoesNotBlockLaterRunForSameParticipant()
    {
        await using var context = CreateContext();
        var (firstRunId, secondRunId, participantId) = await SeedParticipantInTwoRunsWithinSameHeatAsync(context);
        var firstSubmissionId = Guid.NewGuid();
        var secondSubmissionId = Guid.NewGuid();
        var service = CreateService(context);

        var firstRunScore = await service.RegisterScoreAsync(
            new RegisterScoreRequest(firstRunId, participantId, 2, firstSubmissionId),
            CancellationToken.None);
        var secondRunScore = await service.RegisterScoreAsync(
            new RegisterScoreRequest(secondRunId, participantId, 1, secondSubmissionId),
            CancellationToken.None);
        var firstRunRetry = await service.RegisterScoreAsync(
            new RegisterScoreRequest(firstRunId, participantId, 2, firstSubmissionId),
            CancellationToken.None);

        Assert.True(firstRunScore.IsSuccess);
        Assert.True(secondRunScore.IsSuccess);
        Assert.True(firstRunRetry.IsSuccess);
        Assert.Equal(firstRunScore.Value?.Id, firstRunRetry.Value?.Id);
        Assert.Equal(2, context.ScoreEntries.Count(x => x.ParticipantId == participantId));
    }

    [Fact]
    public async Task RegisterScore_PublishesSemanticRealtimeEvent_WithRankChange()
    {
        await using var context = CreateContext();

        var competition = new Competition(Guid.NewGuid(), "Cup", new DateOnly(2026, 3, 20));
        var heat = new Heat(Guid.NewGuid(), competition.Id, 1, 1);
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

    [Fact]
    public async Task RegisterScore_Succeeds_WhenRealtimePublishFails()
    {
        await using var context = CreateContext();
        var (runId, participantId) = await SeedRunParticipantAsync(context);

        var service = CreateService(context, new ThrowingRealtimePublisher());

        var result = await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(context.ScoreEntries);
    }

    [Fact]
    public async Task CorrectScore_Succeeds_WhenRealtimePublishFails()
    {
        await using var context = CreateContext();
        var (runId, participantId) = await SeedRunParticipantAsync(context);
        await context.ScoreEntries.AddAsync(new Domain.Scoring.ScoreEntry(Guid.NewGuid(), runId, participantId, 1, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();

        var service = CreateService(context, new ThrowingRealtimePublisher());

        var result = await service.CorrectScoreAsync(new CorrectScoreRequest(runId, participantId, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.ScoreEntries.Single().Rings);
    }

    private static ScoringService CreateService(ScoreboardDbContext context, IScoreboardRealtimePublisher? publisher = null)
    {
        return new ScoringService(
            context,
            new LeaderboardQueryService(context),
            publisher ?? new RecordingRealtimePublisher(),
            NullLogger<ScoringService>.Instance);
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
        var heat = new Heat(Guid.NewGuid(), competition.Id, 1, 1);
        var run = new Run(Guid.NewGuid(), heat.Id, 1);
        var participant = new Participant(Guid.NewGuid(), competition.Id, 12, "Rider");
        var assignment = new RunParticipant(Guid.NewGuid(), run.Id, participant.Id);

        context.AddRange(competition, heat, run, participant, assignment);
        await context.SaveChangesAsync();

        return (run.Id, participant.Id);
    }

    private static async Task<(Guid FirstRunId, Guid SecondRunId, Guid ParticipantId)> SeedParticipantInTwoRunsWithinSameHeatAsync(
        ScoreboardDbContext context)
    {
        var competition = new Competition(Guid.NewGuid(), "Cup", new DateOnly(2026, 3, 20));
        var heat = new Heat(Guid.NewGuid(), competition.Id, 1, 1);
        var firstRun = new Run(Guid.NewGuid(), heat.Id, 1);
        var secondRun = new Run(Guid.NewGuid(), heat.Id, 2);
        var participant = new Participant(Guid.NewGuid(), competition.Id, 12, "Rider");

        context.AddRange(
            competition,
            heat,
            firstRun,
            secondRun,
            participant,
            new RunParticipant(Guid.NewGuid(), firstRun.Id, participant.Id),
            new RunParticipant(Guid.NewGuid(), secondRun.Id, participant.Id));
        await context.SaveChangesAsync();

        return (firstRun.Id, secondRun.Id, participant.Id);
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

    private sealed class ThrowingRealtimePublisher : IScoreboardRealtimePublisher
    {
        public Task PublishScoreRegisteredAsync(ScoreRegisteredRealtimeEvent @event, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("simulated publish failure");
        }

        public Task PublishScoreCorrectedAsync(ScoreCorrectedRealtimeEvent @event, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("simulated publish failure");
        }
    }
}
