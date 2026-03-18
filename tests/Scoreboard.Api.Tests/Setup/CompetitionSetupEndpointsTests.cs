this_will_not_compile

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scoreboard.Infrastructure.Persistence;
using Xunit;

namespace Scoreboard.Api.Tests.Setup;

public sealed class CompetitionSetupEndpointsTests : IClassFixture<CompetitionSetupWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public CompetitionSetupEndpointsTests(CompetitionSetupWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task FullSetupFlow_CreatesCompetitionParticipantHeatRunAndAssignment()
    {
        var competitionResponse = await _httpClient.PostAsJsonAsync("/api/setup/competitions", new
        {
            name = "Spring Cup",
            competitionDate = "2026-03-17"
        });

        Assert.Equal(HttpStatusCode.Created, competitionResponse.StatusCode);
        var competition = await competitionResponse.Content.ReadFromJsonAsync<CompetitionResponse>();
        Assert.NotNull(competition);

        var participantResponse = await _httpClient.PostAsJsonAsync("/api/setup/participants", new
        {
            competitionId = competition!.Id,
            number = 15,
            name = "Rider"
        });

        Assert.Equal(HttpStatusCode.Created, participantResponse.StatusCode);
        var participant = await participantResponse.Content.ReadFromJsonAsync<ParticipantResponse>();

        var heatResponse = await _httpClient.PostAsJsonAsync("/api/setup/heats", new
        {
            competitionId = competition.Id,
            sequenceNumber = 1
        });

        Assert.Equal(HttpStatusCode.Created, heatResponse.StatusCode);
        var heat = await heatResponse.Content.ReadFromJsonAsync<HeatResponse>();

        var runResponse = await _httpClient.PostAsJsonAsync("/api/setup/runs", new
        {
            heatId = heat!.Id,
            sequenceNumber = 1
        });

        Assert.Equal(HttpStatusCode.Created, runResponse.StatusCode);
        var run = await runResponse.Content.ReadFromJsonAsync<RunResponse>();

        var assignmentResponse = await _httpClient.PostAsJsonAsync("/api/setup/run-assignments", new
        {
            runId = run!.Id,
            participantId = participant!.Id
        });

        Assert.Equal(HttpStatusCode.Created, assignmentResponse.StatusCode);
    }

    private sealed record CompetitionResponse(Guid Id);
    private sealed record ParticipantResponse(Guid Id);
    private sealed record HeatResponse(Guid Id);
    private sealed record RunResponse(Guid Id);
}

public sealed class CompetitionSetupWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<ScoreboardDbContext>));
            services.Remove(descriptor);

            services.AddDbContext<ScoreboardDbContext>(options => options.UseSqlite(_connection));

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ScoreboardDbContext>();
            context.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
