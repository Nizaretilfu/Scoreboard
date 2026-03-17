using Microsoft.EntityFrameworkCore;
using Scoreboard.Domain.Competitions;
using Scoreboard.Domain.Heats;
using Scoreboard.Domain.Participants;
using Scoreboard.Domain.Runs;
using Scoreboard.Domain.Scoring;

namespace Scoreboard.Infrastructure.Persistence;

public sealed class ScoreboardDbContext(DbContextOptions<ScoreboardDbContext> options) : DbContext(options)
{
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Heat> Heats => Set<Heat>();
    public DbSet<Run> Runs => Set<Run>();
    public DbSet<ScoreEntry> ScoreEntries => Set<ScoreEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScoreboardDbContext).Assembly);
    }
}
