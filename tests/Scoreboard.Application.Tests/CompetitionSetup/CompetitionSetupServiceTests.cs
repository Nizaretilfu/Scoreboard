using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scoreboard.Application.Abstractions;
using Scoreboard.Application.CompetitionSetup;
using Scoreboard.Domain.Competitions;
using Scoreboard.Domain.Heats;
using Scoreboard.Domain.Participants;
using Scoreboard.Domain.RunParticipants;
using Scoreboard.Domain.Runs;
using Scoreboard.Domain.Scoring;
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
    public async Task RegisterParticipant_ReturnsConflict_WhenUniqueConstraintIsViolatedOnSave()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ScoreboardDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ScoreboardDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var competitionId = Guid.NewGuid();
        context.Competitions.Add(new Competition(competitionId, "Cup", new DateOnly(2026, 3, 17)));
        context.Participants.Add(new Participant(Guid.NewGuid(), competitionId, 7, "Existing"));
        await context.SaveChangesAsync();

        var service = new CompetitionSetupService(new ConflictOnSaveDbContext(context, competitionId, 7));
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

    private sealed class ConflictOnSaveDbContext(ScoreboardDbContext dbContext, Guid competitionId, int participantNumber)
        : IScoreboardDbContext
    {
        public DbSet<Competition> Competitions => dbContext.Competitions;
        public DbSet<Participant> Participants => dbContext.Participants;
        public DbSet<Heat> Heats => dbContext.Heats;
        public DbSet<Run> Runs => dbContext.Runs;
        public DbSet<RunParticipant> RunParticipants => dbContext.RunParticipants;
        public DbSet<ScoreEntry> ScoreEntries => dbContext.ScoreEntries;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            dbContext.Participants.Add(new Participant(Guid.NewGuid(), competitionId, participantNumber, "Concurrent"));
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
