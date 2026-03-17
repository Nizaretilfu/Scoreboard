namespace Scoreboard.Domain.Runs;

public sealed class Run
{
    public Guid Id { get; private set; }
    public Guid HeatId { get; private set; }
    public int SequenceNumber { get; private set; }

    private Run()
    {
    }

    public Run(Guid id, Guid heatId, int sequenceNumber)
    {
        if (sequenceNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber), "Run sequence number must be greater than zero.");
        }

        Id = id;
        HeatId = heatId;
        SequenceNumber = sequenceNumber;
    }
}
