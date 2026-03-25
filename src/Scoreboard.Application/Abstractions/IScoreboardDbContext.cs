using Microsoft.EntityFrameworkCore;
using Scoreboard.Domain.Competitions;
using Scoreboard.Domain.Heats;
using Scoreboard.Domain.Participants;
using Scoreboard.Domain.RunParticipants;
using Scoreboard.Domain.Runs;
using Scoreboard.Domain.Scoring;

namespace Scoreboard.Application.Abstractions;

public interface IScoreboardDbContext
{
    DbSet<Competition> Competitions { get; }
    DbSet<Participant> Participants { get; }
    DbSet<Heat> Heats { get; }
    DbSet<Run> Runs { get; }
    DbSet<RunParticipant> RunParticipants { get; }
    DbSet<ScoreEntry> ScoreEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
