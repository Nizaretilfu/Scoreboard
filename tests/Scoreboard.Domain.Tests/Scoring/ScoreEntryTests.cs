using Scoreboard.Domain.Scoring;
using Xunit;

namespace Scoreboard.Domain.Tests.Scoring;

public sealed class ScoreEntryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Constructor_AllowsRingsWithinRange(int rings)
    {
        var scoreEntry = new ScoreEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), rings, DateTimeOffset.UtcNow);

        Assert.Equal(rings, scoreEntry.Rings);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Constructor_ThrowsForInvalidRings(int rings)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ScoreEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), rings, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CorrectScore_UpdatesRings_WhenValueIsValid()
    {
        var scoreEntry = new ScoreEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, DateTimeOffset.UtcNow);

        scoreEntry.CorrectScore(2);

        Assert.Equal(2, scoreEntry.Rings);
    }
}
