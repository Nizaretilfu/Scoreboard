namespace Scoreboard.Domain.Heats;

public sealed class Heat
{
    public Guid Id { get; private set; }
    public Guid CompetitionId { get; private set; }
    public int SequenceNumber { get; private set; }

    private Heat()
    {
    }

    public Heat(Guid id, Guid competitionId, int sequenceNumber)
    {
        if (sequenceNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber), "Heat sequence number must be greater than zero.");
        }

        Id = id;
        CompetitionId = competitionId;
        SequenceNumber = sequenceNumber;
    }
}
