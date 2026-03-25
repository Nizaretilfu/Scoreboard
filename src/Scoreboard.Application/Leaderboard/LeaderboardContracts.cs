namespace Scoreboard.Application.Leaderboard;

public sealed record GetCompetitionLeaderboardRequest(Guid CompetitionId);

public sealed record LeaderboardRowDto(
    int Rank,
    Guid ParticipantId,
    int ParticipantNumber,
    string ParticipantName,
    int TotalRings);

public sealed record CompetitionLeaderboardDto(
    Guid CompetitionId,
    IReadOnlyList<LeaderboardRowDto> Rows,
    DateTimeOffset GeneratedAtUtc);
