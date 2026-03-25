namespace Scoreboard.Api.Contracts;

public sealed record CreateCompetitionApiRequest(string Name, DateOnly CompetitionDate);
public sealed record RegisterParticipantApiRequest(Guid CompetitionId, int Number, string Name);
public sealed record CreateHeatApiRequest(Guid CompetitionId, int SequenceNumber);
public sealed record CreateRunApiRequest(Guid HeatId, int SequenceNumber);
public sealed record AssignParticipantToRunApiRequest(Guid RunId, Guid ParticipantId);

public sealed record CompetitionOverviewApiResponse(Guid Id, string Name, DateOnly CompetitionDate);
public sealed record RunParticipantOverviewApiResponse(Guid ParticipantId, int ParticipantNumber, string ParticipantName);
public sealed record CompetitionRunOverviewApiResponse(
    Guid RunId,
    Guid HeatId,
    int HeatSequenceNumber,
    int RunSequenceNumber,
    IReadOnlyList<RunParticipantOverviewApiResponse> Participants);
