using Scoreboard.Application.CompetitionSetup;

namespace Scoreboard.Application.Scoring;

public sealed record RegisterScoreRequest(Guid RunId, Guid ParticipantId, int Rings);
public sealed record CorrectScoreRequest(Guid RunId, Guid ParticipantId, int Rings);

public sealed record ScoreEntryDto(Guid Id, Guid RunId, Guid ParticipantId, int Rings, DateTimeOffset RegisteredAtUtc);
