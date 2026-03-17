using Microsoft.EntityFrameworkCore;
using Scoreboard.Application.Abstractions;
using Scoreboard.Domain.Competitions;
using Scoreboard.Domain.Heats;
using Scoreboard.Domain.Participants;
using Scoreboard.Domain.RunParticipants;
using Scoreboard.Domain.Runs;

namespace Scoreboard.Application.CompetitionSetup;

public sealed class CompetitionSetupService(IScoreboardDbContext dbContext)
{
    public async Task<OperationResult<CompetitionDto>> CreateCompetitionAsync(CreateCompetitionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var competition = new Competition(Guid.NewGuid(), request.Name, request.CompetitionDate);
            dbContext.Competitions.Add(competition);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<CompetitionDto>.Success(new CompetitionDto(competition.Id, competition.Name, competition.CompetitionDate));
        }
        catch (ArgumentException ex)
        {
            return OperationResult<CompetitionDto>.Failure("validation_error", ex.Message);
        }
    }

    public async Task<OperationResult<ParticipantDto>> RegisterParticipantAsync(RegisterParticipantRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Competitions.AnyAsync(c => c.Id == request.CompetitionId, cancellationToken))
        {
            return OperationResult<ParticipantDto>.Failure("competition_not_found", "Competition not found.");
        }

        if (await dbContext.Participants.AnyAsync(p => p.CompetitionId == request.CompetitionId && p.Number == request.Number, cancellationToken))
        {
            return OperationResult<ParticipantDto>.Failure("participant_number_conflict", "Participant number already exists in this competition.");
        }

        try
        {
            var participant = new Participant(Guid.NewGuid(), request.CompetitionId, request.Number, request.Name);
            dbContext.Participants.Add(participant);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<ParticipantDto>.Success(
                new ParticipantDto(participant.Id, participant.CompetitionId, participant.Number, participant.Name));
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return OperationResult<ParticipantDto>.Failure("participant_number_conflict", "Participant number already exists in this competition.");
        }
        catch (ArgumentException ex)
        {
            return OperationResult<ParticipantDto>.Failure("validation_error", ex.Message);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var innerException = exception.InnerException;
        if (innerException is null)
        {
            return false;
        }

        var innerType = innerException.GetType();
        if (innerType.Name == "PostgresException")
        {
            var sqlState = innerType.GetProperty("SqlState")?.GetValue(innerException)?.ToString();
            return string.Equals(sqlState, "23505", StringComparison.Ordinal);
        }

        if (innerType.Name == "SqliteException")
        {
            var sqliteErrorCode = innerType.GetProperty("SqliteErrorCode")?.GetValue(innerException) as int?;
            var sqliteExtendedErrorCode = innerType.GetProperty("SqliteExtendedErrorCode")?.GetValue(innerException) as int?;
            return sqliteErrorCode == 19 || sqliteExtendedErrorCode is 1555 or 2067;
        }

        return innerException.Message.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || innerException.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<OperationResult<HeatDto>> CreateHeatAsync(CreateHeatRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Competitions.AnyAsync(c => c.Id == request.CompetitionId, cancellationToken))
        {
            return OperationResult<HeatDto>.Failure("competition_not_found", "Competition not found.");
        }

        if (await dbContext.Heats.AnyAsync(h => h.CompetitionId == request.CompetitionId && h.SequenceNumber == request.SequenceNumber, cancellationToken))
        {
            return OperationResult<HeatDto>.Failure("heat_sequence_conflict", "Heat sequence number already exists in this competition.");
        }

        try
        {
            var heat = new Heat(Guid.NewGuid(), request.CompetitionId, request.SequenceNumber);
            dbContext.Heats.Add(heat);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<HeatDto>.Success(new HeatDto(heat.Id, heat.CompetitionId, heat.SequenceNumber));
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return OperationResult<HeatDto>.Failure("heat_sequence_conflict", "Heat sequence number already exists in this competition.");
        }
        catch (ArgumentException ex)
        {
            return OperationResult<HeatDto>.Failure("validation_error", ex.Message);
        }
    }

    public async Task<OperationResult<RunDto>> CreateRunAsync(CreateRunRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Heats.AnyAsync(h => h.Id == request.HeatId, cancellationToken))
        {
            return OperationResult<RunDto>.Failure("heat_not_found", "Heat not found.");
        }

        if (await dbContext.Runs.AnyAsync(r => r.HeatId == request.HeatId && r.SequenceNumber == request.SequenceNumber, cancellationToken))
        {
            return OperationResult<RunDto>.Failure("run_sequence_conflict", "Run sequence number already exists in this heat.");
        }

        try
        {
            var run = new Run(Guid.NewGuid(), request.HeatId, request.SequenceNumber);
            dbContext.Runs.Add(run);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<RunDto>.Success(new RunDto(run.Id, run.HeatId, run.SequenceNumber));
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return OperationResult<RunDto>.Failure("run_sequence_conflict", "Run sequence number already exists in this heat.");
        }
        catch (ArgumentException ex)
        {
            return OperationResult<RunDto>.Failure("validation_error", ex.Message);
        }
    }

    public async Task<OperationResult<RunParticipantDto>> AssignParticipantToRunAsync(AssignParticipantToRunRequest request, CancellationToken cancellationToken)
    {
        var run = await dbContext.Runs.SingleOrDefaultAsync(r => r.Id == request.RunId, cancellationToken);
        if (run is null)
        {
            return OperationResult<RunParticipantDto>.Failure("run_not_found", "Run not found.");
        }

        var participant = await dbContext.Participants.SingleOrDefaultAsync(p => p.Id == request.ParticipantId, cancellationToken);
        if (participant is null)
        {
            return OperationResult<RunParticipantDto>.Failure("participant_not_found", "Participant not found.");
        }

        var runCompetitionId = await dbContext.Heats
            .Where(h => h.Id == run.HeatId)
            .Select(h => h.CompetitionId)
            .SingleAsync(cancellationToken);

        if (runCompetitionId != participant.CompetitionId)
        {
            return OperationResult<RunParticipantDto>.Failure("competition_mismatch", "Participant and run must belong to the same competition.");
        }

        if (await dbContext.RunParticipants.AnyAsync(rp => rp.RunId == request.RunId && rp.ParticipantId == request.ParticipantId, cancellationToken))
        {
            return OperationResult<RunParticipantDto>.Failure("assignment_conflict", "Participant is already assigned to this run.");
        }

        try
        {
            var assignment = new RunParticipant(Guid.NewGuid(), request.RunId, request.ParticipantId);
            dbContext.RunParticipants.Add(assignment);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<RunParticipantDto>.Success(new RunParticipantDto(assignment.Id, assignment.RunId, assignment.ParticipantId));
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return OperationResult<RunParticipantDto>.Failure("assignment_conflict", "Participant is already assigned to this run.");
        }
    }
}
