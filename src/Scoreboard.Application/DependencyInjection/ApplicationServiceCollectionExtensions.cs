using Microsoft.Extensions.DependencyInjection;
using Scoreboard.Application.CompetitionSetup;
using Scoreboard.Application.Leaderboard;
using Scoreboard.Application.Realtime;
using Scoreboard.Application.Scoring;

namespace Scoreboard.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CompetitionSetupService>();
        services.AddScoped<LeaderboardQueryService>();
        services.AddScoped<ScoringService>();
        services.AddScoped<IScoreboardRealtimePublisher, NullScoreboardRealtimePublisher>();
        return services;
    }
}
