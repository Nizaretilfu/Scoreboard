using Microsoft.EntityFrameworkCore;
using Scoreboard.Application.CompetitionSetup;
using Scoreboard.Domain.Competitions;
using Scoreboard.Domain.Heats;
using Scoreboard.Domain.Participants;
using Scoreboard.Domain.Runs;
using Scoreboard.Infrastructure.Persistence;
using Xunit;

namespace Scoreboard.Application.Tests.CompetitionSetup;

public sealed class CompetitionSetupServiceTests
{
    [Fact]
    public async Task RegisterParticipant_ReturnsConflict_WhenNumberAlreadyExistsInCompetition()
    {
        await using var context = CreateContext();
        var competitionId = Guid.NewGuid();

        context.Competitions.Add(new Competition(competitionId, "Cup", new DateOnly(2026, 3, 17)));
        context.Participants.Add(new Participant(Guid.NewGuid(), competitionId, 7, "Existing"));
        await context.SaveChangesAsync();

        var service = new CompetitionSetupService(context);
        var result = await service.RegisterParticipantAsync(new RegisterParticipantRequest(competitionId, 7, "New"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("participant_number_conflict", result.Error?.Code);
    }

    [Fact]
    public async Task AssignParticipantToRun_ReturnsCompetitionMismatch_WhenEntitiesBelongToDifferentCompetitions()
    {
        await using var context = CreateContext();
        var competition1 = new Competition(Guid.NewGuid(), "Cup 1", new DateOnly(2026, 3, 17));
        var competition2 = new Competition(Guid.NewGuid(), "Cup 2", new DateOnly(2026, 3, 17));
        var heat = new Heat(Guid.NewGuid(), competition1.Id, 1);
        var run = new Run(Guid.NewGuid(), heat.Id, 1);
        var participant = new Participant(Guid.NewGuid(), competition2.Id, 10, "Rider");

        context.AddRange(competition1, competition2, heat, run, participant);
        await context.SaveChangesAsync();

        var service = new CompetitionSetupService(context);
        var result = await service.AssignParticipantToRunAsync(new AssignParticipantToRunRequest(run.Id, participant.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("competition_mismatch", result.Error?.Code);
    }

    private static ScoreboardDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ScoreboardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ScoreboardDbContext(options);
    }
}
