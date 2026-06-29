namespace Scoreboard.Domain.Heats;

public sealed class Heat
{
    public const int MaxConfiguredRunCount = 100;

    public Guid Id { get; private set; }
    public Guid CompetitionId { get; private set; }
    public int SequenceNumber { get; private set; }
    public int ConfiguredRunCount { get; private set; }

    private Heat()
    {
    }

    public Heat(Guid id, Guid competitionId, int sequenceNumber, int configuredRunCount)
    {
        if (sequenceNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber), "Heat sequence number must be greater than zero.");
        }

        if (configuredRunCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(configuredRunCount), "Configured run count must be greater than zero.");
        }

        if (configuredRunCount > MaxConfiguredRunCount)
        {
            throw new ArgumentOutOfRangeException(nameof(configuredRunCount), $"Configured run count must be {MaxConfiguredRunCount} or less.");
        }

        Id = id;
        CompetitionId = competitionId;
        SequenceNumber = sequenceNumber;
        ConfiguredRunCount = configuredRunCount;
    }
}
