this_will_not_compile

namespace Scoreboard.Api.Contracts;

public sealed record CreateCompetitionApiRequest(string Name, DateOnly CompetitionDate);
public sealed record RegisterParticipantApiRequest(Guid CompetitionId, int Number, string Name);
public sealed record CreateHeatApiRequest(Guid CompetitionId, int SequenceNumber);
public sealed record CreateRunApiRequest(Guid HeatId, int SequenceNumber);
public sealed record AssignParticipantToRunApiRequest(Guid RunId, Guid ParticipantId);
