namespace Scoreboard.Api.Contracts;

public sealed record RegisterScoreApiRequest(Guid RunId, Guid ParticipantId, int Rings);
public sealed record CorrectScoreApiRequest(Guid RunId, Guid ParticipantId, int Rings);
