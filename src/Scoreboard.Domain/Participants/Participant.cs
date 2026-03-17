namespace Scoreboard.Domain.Participants;

public sealed class Participant
{
    public Guid Id { get; private set; }
    public Guid CompetitionId { get; private set; }
    public int Number { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Participant()
    {
    }

    public Participant(Guid id, Guid competitionId, int number, string name)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Participant number must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Participant name is required.", nameof(name));
        }

        Id = id;
        CompetitionId = competitionId;
        Number = number;
        Name = name.Trim();
    }
}
