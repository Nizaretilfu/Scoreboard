using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Scoreboard.Infrastructure.Persistence;

public sealed class ScoreboardDbContextFactory : IDesignTimeDbContextFactory<ScoreboardDbContext>
{
    private const string ConnectionStringName = "ScoreboardDatabase";

    public ScoreboardDbContext CreateDbContext(string[] args)
    {
        var connectionString = BuildConfiguration().GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is required for design-time operations.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ScoreboardDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new ScoreboardDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var currentDirectory = Directory.GetCurrentDirectory();
        var apiProjectPath = Path.GetFullPath(Path.Combine(currentDirectory, "src", "Scoreboard.Api"));

        var builder = new ConfigurationBuilder()
            .SetBasePath(currentDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        if (!string.IsNullOrWhiteSpace(environmentName))
        {
            builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
        }

        if (Directory.Exists(apiProjectPath))
        {
            builder.AddJsonFile(Path.Combine(apiProjectPath, "appsettings.json"), optional: true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                builder.AddJsonFile(Path.Combine(apiProjectPath, $"appsettings.{environmentName}.json"), optional: true);
            }
        }

        return builder.Build();
    }
}
