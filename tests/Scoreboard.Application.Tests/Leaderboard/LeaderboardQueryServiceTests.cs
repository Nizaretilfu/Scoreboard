using Microsoft.EntityFrameworkCore;
using Scoreboard.Application.Leaderboard;
using Scoreboard.Domain.Competitions;
using Scoreboard.Domain.Heats;
using Scoreboard.Domain.Participants;
using Scoreboard.Domain.RunParticipants;
using Scoreboard.Domain.Runs;
using Scoreboard.Domain.Scoring;
using Scoreboard.Infrastructure.Persistence;
using Xunit;

namespace Scoreboard.Application.Tests.Leaderboard;

public sealed class LeaderboardQueryServiceTests
{
    [Fact]
    public async Task GetCompetitionLeaderboard_AppliesDeterministicRanking_WithTieBreakByParticipantNumber()
    {
        await using var context = CreateContext();

        var competition = new Competition(Guid.NewGuid(), "Cup", new DateOnly(2026, 3, 20));
        var heat = new Heat(Guid.NewGuid(), competition.Id, 1);
        var run = new Run(Guid.NewGuid(), heat.Id, 1);

        var participantA = new Participant(Guid.NewGuid(), competition.Id, 10, "A");
        var participantB = new Participant(Guid.NewGuid(), competition.Id, 11, "B");
        var participantC = new Participant(Guid.NewGuid(), competition.Id, 9, "C");

        context.AddRange(
            competition,
            heat,
            run,
            participantA,
            participantB,
            participantC,
            new RunParticipant(Guid.NewGuid(), run.Id, participantA.Id),
            new RunParticipant(Guid.NewGuid(), run.Id, participantB.Id),
            new RunParticipant(Guid.NewGuid(), run.Id, participantC.Id),
            new ScoreEntry(Guid.NewGuid(), run.Id, participantA.Id, 2, DateTimeOffset.UtcNow),
            new ScoreEntry(Guid.NewGuid(), run.Id, participantB.Id, 1, DateTimeOffset.UtcNow),
            new ScoreEntry(Guid.NewGuid(), run.Id, participantC.Id, 1, DateTimeOffset.UtcNow));

        await context.SaveChangesAsync();

        var service = new LeaderboardQueryService(context);
        var leaderboard = await service.GetCompetitionLeaderboardAsync(new GetCompetitionLeaderboardRequest(competition.Id), CancellationToken.None);

        Assert.Equal(new[] { participantA.Id, participantC.Id, participantB.Id }, leaderboard.Rows.Select(x => x.ParticipantId));
        Assert.Equal(new[] { 1, 2, 3 }, leaderboard.Rows.Select(x => x.Rank));
    }

    private static ScoreboardDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ScoreboardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ScoreboardDbContext(options);
    }
}
