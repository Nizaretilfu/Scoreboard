using Microsoft.EntityFrameworkCore;
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
        var service = new ScoringService(context);

        var result = await service.RegisterScoreAsync(new RegisterScoreRequest(Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("run_participant_not_found", result.Error?.Code);
    }

    [Fact]
    public async Task RegisterScore_ReturnsConflict_WhenScoreAlreadyExists()
    {
        await using var context = CreateContext();
        var (runId, participantId) = await SeedRunParticipantAsync(context);

        var service = new ScoringService(context);
        var first = await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 1), CancellationToken.None);
        Assert.True(first.IsSuccess);

        var second = await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 2), CancellationToken.None);

        Assert.False(second.IsSuccess);
        Assert.Equal("score_already_registered", second.Error?.Code);
    }

    [Fact]
    public async Task CorrectScore_UpdatesRings_WhenScoreExists()
    {
        await using var context = CreateContext();
        var (runId, participantId) = await SeedRunParticipantAsync(context);

        var service = new ScoringService(context);
        await service.RegisterScoreAsync(new RegisterScoreRequest(runId, participantId, 0), CancellationToken.None);

        var correction = await service.CorrectScoreAsync(new CorrectScoreRequest(runId, participantId, 2), CancellationToken.None);

        Assert.True(correction.IsSuccess);
        Assert.Equal(2, correction.Value?.Rings);
    }

    [Fact]
    public async Task CorrectScore_ReturnsNotFound_WhenScoreDoesNotExist()
    {
        await using var context = CreateContext();
        var service = new ScoringService(context);

        var result = await service.CorrectScoreAsync(new CorrectScoreRequest(Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("score_not_found", result.Error?.Code);
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
}
