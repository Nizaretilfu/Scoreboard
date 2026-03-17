using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scoreboard.Infrastructure.Persistence;

public sealed class ScoreboardDbContextFactory : IDesignTimeDbContextFactory<ScoreboardDbContext>
{
    public ScoreboardDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=scoreboard;Username=postgres;Password=postgres";
        var optionsBuilder = new DbContextOptionsBuilder<ScoreboardDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new ScoreboardDbContext(optionsBuilder.Options);
    }
}
