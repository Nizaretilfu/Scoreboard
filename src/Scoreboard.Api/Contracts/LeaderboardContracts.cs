namespace Scoreboard.Api.Contracts;

public sealed record LeaderboardRowApiResponse(
    int Rank,
    Guid ParticipantId,
    int ParticipantNumber,
    string ParticipantName,
    int TotalRings);

public sealed record CompetitionLeaderboardApiResponse(
    Guid CompetitionId,
    IReadOnlyList<LeaderboardRowApiResponse> Rows,
    DateTimeOffset GeneratedAtUtc);
