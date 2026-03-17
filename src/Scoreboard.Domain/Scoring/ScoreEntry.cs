namespace Scoreboard.Domain.Scoring;

public sealed class ScoreEntry
{
    public Guid Id { get; private set; }
    public Guid RunId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public int Rings { get; private set; }
    public DateTimeOffset RegisteredAtUtc { get; private set; }

    private ScoreEntry()
    {
    }

    public ScoreEntry(Guid id, Guid runId, Guid participantId, int rings, DateTimeOffset registeredAtUtc)
    {
        EnsureValidRings(rings);

        Id = id;
        RunId = runId;
        ParticipantId = participantId;
        Rings = rings;
        RegisteredAtUtc = registeredAtUtc;
    }

    public void CorrectScore(int rings)
    {
        EnsureValidRings(rings);
        Rings = rings;
    }

    private static void EnsureValidRings(int rings)
    {
        if (rings < 0 || rings > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(rings), "Rings must be between 0 and 2 for ring riding.");
        }
    }
}
