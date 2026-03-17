using Microsoft.Extensions.DependencyInjection;
using Scoreboard.Application.CompetitionSetup;

namespace Scoreboard.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CompetitionSetupService>();
        return services;
    }
}
