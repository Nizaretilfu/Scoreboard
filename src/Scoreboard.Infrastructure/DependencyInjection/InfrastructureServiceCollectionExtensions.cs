using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scoreboard.Infrastructure.Persistence;

namespace Scoreboard.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    private const string ConnectionStringName = "ScoreboardDatabase";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is required.");
        }

        services.AddDbContext<ScoreboardDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
