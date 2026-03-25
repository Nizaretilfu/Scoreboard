using Microsoft.EntityFrameworkCore;
using Scoreboard.Application.Abstractions;
using Scoreboard.Application.CompetitionSetup;
using Scoreboard.Domain.Scoring;

namespace Scoreboard.Application.Scoring;

public sealed class ScoringService(IScoreboardDbContext dbContext)
{
    public async Task<OperationResult<ScoreEntryDto>> RegisterScoreAsync(RegisterScoreRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.RunParticipants.AnyAsync(
                rp => rp.RunId == request.RunId && rp.ParticipantId == request.ParticipantId,
                cancellationToken))
        {
            return OperationResult<ScoreEntryDto>.Failure(
                "run_participant_not_found",
                "Participant must be assigned to the run before scoring.");
        }

        if (await dbContext.ScoreEntries.AnyAsync(
                s => s.RunId == request.RunId && s.ParticipantId == request.ParticipantId,
                cancellationToken))
        {
            return OperationResult<ScoreEntryDto>.Failure(
                "score_already_registered",
                "Score is already registered for this participant in this run.");
        }

        try
        {
            var entry = new ScoreEntry(Guid.NewGuid(), request.RunId, request.ParticipantId, request.Rings, DateTimeOffset.UtcNow);
            dbContext.ScoreEntries.Add(entry);
            await dbContext.SaveChangesAsync(cancellationToken);

            return OperationResult<ScoreEntryDto>.Success(ToDto(entry));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return OperationResult<ScoreEntryDto>.Failure("validation_error", ex.Message);
        }
    }

    public async Task<OperationResult<ScoreEntryDto>> CorrectScoreAsync(CorrectScoreRequest request, CancellationToken cancellationToken)
    {
        var entry = await dbContext.ScoreEntries.SingleOrDefaultAsync(
            s => s.RunId == request.RunId && s.ParticipantId == request.ParticipantId,
            cancellationToken);

        if (entry is null)
        {
            return OperationResult<ScoreEntryDto>.Failure("score_not_found", "Score not found for this participant in this run.");
        }

        try
        {
            entry.CorrectScore(request.Rings);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<ScoreEntryDto>.Success(ToDto(entry));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return OperationResult<ScoreEntryDto>.Failure("validation_error", ex.Message);
        }
    }

    private static ScoreEntryDto ToDto(ScoreEntry entry) =>
        new(entry.Id, entry.RunId, entry.ParticipantId, entry.Rings, entry.RegisteredAtUtc);
}
