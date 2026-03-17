using Microsoft.EntityFrameworkCore;
using Scoreboard.Domain.Scoring;
using Scoreboard.Infrastructure.Persistence;
using Xunit;

namespace Scoreboard.Infrastructure.Tests.Persistence;

public sealed class ScoreboardDbContextModelTests
{
    [Fact]
    public void ScoreEntry_HasRingsCheckConstraint()
    {
        var options = new DbContextOptionsBuilder<ScoreboardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ScoreboardDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(ScoreEntry));
        var checkConstraints = entityType!.GetCheckConstraints();

        Assert.Contains(checkConstraints, c => c.Name == "ck_score_entries_rings");
    }

    [Fact]
    public void ScoreEntry_HasUniqueIndexForRunAndParticipant()
    {
        var options = new DbContextOptionsBuilder<ScoreboardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ScoreboardDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(ScoreEntry));
        var uniqueIndex = entityType!.GetIndexes().SingleOrDefault(i =>
            i.IsUnique &&
            i.Properties.Select(p => p.Name).SequenceEqual(new[] { "RunId", "ParticipantId" }));

        Assert.NotNull(uniqueIndex);
    }
}
