using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Scoreboard.Api.Tests.Scoring;

public sealed class ScoringEndpointsTests : IClassFixture<Setup.CompetitionSetupWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public ScoringEndpointsTests(Setup.CompetitionSetupWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterScore_ReturnsUnauthorized_WhenApiKeyIsMissing()
    {
        var setup = await CreateAssignedParticipantAsync();

        var response = await _httpClient.PostAsJsonAsync("/api/scoring/scores", new
        {
            runId = setup.RunId,
            participantId = setup.ParticipantId,
            rings = 1
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAndCorrectScore_ReturnExpectedStatusCodes_WhenAuthorized()
    {
        var setup = await CreateAssignedParticipantAsync();
        _httpClient.DefaultRequestHeaders.Remove("X-Api-Key");
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", "dev-scoreboard-key");

        var registerResponse = await _httpClient.PostAsJsonAsync("/api/scoring/scores", new
        {
            runId = setup.RunId,
            participantId = setup.ParticipantId,
            rings = 1
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var correctResponse = await _httpClient.PutAsJsonAsync("/api/scoring/scores", new
        {
            runId = setup.RunId,
            participantId = setup.ParticipantId,
            rings = 2
        });

        Assert.Equal(HttpStatusCode.OK, correctResponse.StatusCode);
    }

    private async Task<(Guid RunId, Guid ParticipantId)> CreateAssignedParticipantAsync()
    {
        var competitionResponse = await _httpClient.PostAsJsonAsync("/api/setup/competitions", new
        {
            name = $"Cup-{Guid.NewGuid():N}",
            competitionDate = "2026-03-20"
        });

        var competition = await competitionResponse.Content.ReadFromJsonAsync<IdResponse>();

        var participantResponse = await _httpClient.PostAsJsonAsync("/api/setup/participants", new
        {
            competitionId = competition!.Id,
            number = Random.Shared.Next(1000, 9999),
            name = "Rider"
        });

        var participant = await participantResponse.Content.ReadFromJsonAsync<IdResponse>();

        var heatResponse = await _httpClient.PostAsJsonAsync("/api/setup/heats", new
        {
            competitionId = competition.Id,
            sequenceNumber = 1
        });

        var heat = await heatResponse.Content.ReadFromJsonAsync<IdResponse>();

        var runResponse = await _httpClient.PostAsJsonAsync("/api/setup/runs", new
        {
            heatId = heat!.Id,
            sequenceNumber = 1
        });

        var run = await runResponse.Content.ReadFromJsonAsync<IdResponse>();

        await _httpClient.PostAsJsonAsync("/api/setup/run-assignments", new
        {
            runId = run!.Id,
            participantId = participant!.Id
        });

        return (run.Id, participant.Id);
    }

    private sealed record IdResponse(Guid Id);
}
