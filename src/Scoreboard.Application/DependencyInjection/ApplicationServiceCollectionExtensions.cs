using Microsoft.Extensions.DependencyInjection;
using Scoreboard.Application.CompetitionSetup;
using Scoreboard.Application.Scoring;

namespace Scoreboard.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CompetitionSetupService>();
        services.AddScoped<ScoringService>();
        return services;
    }
}
