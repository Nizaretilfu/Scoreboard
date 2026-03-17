namespace Scoreboard.Domain.RunParticipants;

public sealed class RunParticipant
{
    public Guid Id { get; private set; }
    public Guid RunId { get; private set; }
    public Guid ParticipantId { get; private set; }

    private RunParticipant()
    {
    }

    public RunParticipant(Guid id, Guid runId, Guid participantId)
    {
        Id = id;
        RunId = runId;
        ParticipantId = participantId;
    }
}
