namespace Scoreboard.Application.CompetitionSetup;

public sealed record CreateCompetitionRequest(string Name, DateOnly CompetitionDate);
public sealed record RegisterParticipantRequest(Guid CompetitionId, int Number, string Name);
public sealed record CreateHeatRequest(Guid CompetitionId, int SequenceNumber);
public sealed record CreateRunRequest(Guid HeatId, int SequenceNumber);
public sealed record AssignParticipantToRunRequest(Guid RunId, Guid ParticipantId);

public sealed record CompetitionDto(Guid Id, string Name, DateOnly CompetitionDate);
public sealed record ParticipantDto(Guid Id, Guid CompetitionId, int Number, string Name);
public sealed record HeatDto(Guid Id, Guid CompetitionId, int SequenceNumber);
public sealed record RunDto(Guid Id, Guid HeatId, int SequenceNumber);
public sealed record RunParticipantDto(Guid Id, Guid RunId, Guid ParticipantId);

public sealed record OperationError(string Code, string Message);
public sealed record OperationResult<T>(T? Value, OperationError? Error)
{
    public bool IsSuccess => Error is null;

    public static OperationResult<T> Success(T value) => new(value, null);
    public static OperationResult<T> Failure(string code, string message) => new(default, new OperationError(code, message));
}
