using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Scoreboard.Api.Tests.Leaderboard;

public sealed class LeaderboardEndpointsTests : IClassFixture<Setup.CompetitionSetupWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public LeaderboardEndpointsTests(Setup.CompetitionSetupWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetCompetitionLeaderboard_ReturnsDeterministicRanking()
    {
        var setup = await CreateCompetitionWithScoresAsync();

        var response = await _httpClient.GetAsync($"/api/leaderboard/competitions/{setup.CompetitionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var leaderboard = await response.Content.ReadFromJsonAsync<CompetitionLeaderboardResponse>();
        Assert.NotNull(leaderboard);

        Assert.Equal(new[] { setup.ParticipantAId, setup.ParticipantCId, setup.ParticipantBId }, leaderboard!.Rows.Select(x => x.ParticipantId));
        Assert.Equal(new[] { 1, 2, 3 }, leaderboard.Rows.Select(x => x.Rank));
    }

    private async Task<(Guid CompetitionId, Guid ParticipantAId, Guid ParticipantBId, Guid ParticipantCId)> CreateCompetitionWithScoresAsync()
    {
        _httpClient.DefaultRequestHeaders.Remove("X-Api-Key");
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", "dev-scoreboard-key");

        var competition = await (await _httpClient.PostAsJsonAsync("/api/setup/competitions", new
        {
            name = $"Cup-{Guid.NewGuid():N}",
            competitionDate = "2026-03-20"
        })).Content.ReadFromJsonAsync<IdResponse>();

        var participantA = await CreateParticipantAsync(competition!.Id, 10, "A");
        var participantB = await CreateParticipantAsync(competition.Id, 11, "B");
        var participantC = await CreateParticipantAsync(competition.Id, 9, "C");

        var heat = await (await _httpClient.PostAsJsonAsync("/api/setup/heats", new
        {
            competitionId = competition.Id,
            sequenceNumber = 1
        })).Content.ReadFromJsonAsync<IdResponse>();

        var run = await (await _httpClient.PostAsJsonAsync("/api/setup/runs", new
        {
            heatId = heat!.Id,
            sequenceNumber = 1
        })).Content.ReadFromJsonAsync<IdResponse>();

        await _httpClient.PostAsJsonAsync("/api/setup/run-assignments", new { runId = run!.Id, participantId = participantA.Id });
        await _httpClient.PostAsJsonAsync("/api/setup/run-assignments", new { runId = run.Id, participantId = participantB.Id });
        await _httpClient.PostAsJsonAsync("/api/setup/run-assignments", new { runId = run.Id, participantId = participantC.Id });

        await _httpClient.PostAsJsonAsync("/api/scoring/scores", new { runId = run.Id, participantId = participantA.Id, rings = 2 });
        await _httpClient.PostAsJsonAsync("/api/scoring/scores", new { runId = run.Id, participantId = participantB.Id, rings = 1 });
        await _httpClient.PostAsJsonAsync("/api/scoring/scores", new { runId = run.Id, participantId = participantC.Id, rings = 1 });

        return (competition.Id, participantA.Id, participantB.Id, participantC.Id);
    }

    private async Task<IdResponse> CreateParticipantAsync(Guid competitionId, int number, string name)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/setup/participants", new
        {
            competitionId,
            number,
            name
        });

        return (await response.Content.ReadFromJsonAsync<IdResponse>())!;
    }

    private sealed record IdResponse(Guid Id);

    private sealed record CompetitionLeaderboardResponse(Guid CompetitionId, IReadOnlyList<LeaderboardRowResponse> Rows);

    private sealed record LeaderboardRowResponse(int Rank, Guid ParticipantId);
}
