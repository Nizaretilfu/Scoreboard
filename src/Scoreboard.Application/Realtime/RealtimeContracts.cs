using Scoreboard.Application.Leaderboard;

namespace Scoreboard.Application.Realtime;

public sealed record ParticipantRankChangedEvent(
    Guid ParticipantId,
    int PreviousRank,
    int NewRank);

public sealed record ScoreRegisteredRealtimeEvent(
    Guid CompetitionId,
    Guid RunId,
    Guid ParticipantId,
    int Rings,
    CompetitionLeaderboardDto Leaderboard,
    ParticipantRankChangedEvent? RankChanged);

public sealed record ScoreCorrectedRealtimeEvent(
    Guid CompetitionId,
    Guid RunId,
    Guid ParticipantId,
    int PreviousRings,
    int Rings,
    CompetitionLeaderboardDto Leaderboard,
    ParticipantRankChangedEvent? RankChanged);
