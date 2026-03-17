namespace Scoreboard.Domain.Competitions;

public sealed class Competition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly CompetitionDate { get; private set; }

    private Competition()
    {
    }

    public Competition(Guid id, string name, DateOnly competitionDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Competition name is required.", nameof(name));
        }

        Id = id;
        Name = name.Trim();
        CompetitionDate = competitionDate;
    }
}
