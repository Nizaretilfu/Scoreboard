using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scoreboard.Application.Abstractions;
using Scoreboard.Application.CompetitionSetup;
using Scoreboard.Application.Leaderboard;
using Scoreboard.Application.Realtime;
using Scoreboard.Domain.Scoring;

namespace Scoreboard.Application.Scoring;

public sealed class ScoringService(
    IScoreboardDbContext dbContext,
    LeaderboardQueryService leaderboardQueryService,
    IScoreboardRealtimePublisher realtimePublisher,
    ILogger<ScoringService> logger)
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

        var competitionId = await ResolveCompetitionIdForRunAsync(request.RunId, cancellationToken);
        if (competitionId is null)
        {
            return OperationResult<ScoreEntryDto>.Failure("run_not_found", "Run not found.");
        }

        try
        {
            var entry = new ScoreEntry(Guid.NewGuid(), request.RunId, request.ParticipantId, request.Rings, DateTimeOffset.UtcNow);
            dbContext.ScoreEntries.Add(entry);
            await dbContext.SaveChangesAsync(cancellationToken);

            await TryPublishScoreRegisteredAsync(competitionId.Value, request);

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

        var competitionId = await ResolveCompetitionIdForRunAsync(request.RunId, cancellationToken);
        if (competitionId is null)
        {
            return OperationResult<ScoreEntryDto>.Failure("run_not_found", "Run not found.");
        }

        var previousRings = entry.Rings;

        try
        {
            entry.CorrectScore(request.Rings);
            await dbContext.SaveChangesAsync(cancellationToken);

            await TryPublishScoreCorrectedAsync(competitionId.Value, request, previousRings);

            return OperationResult<ScoreEntryDto>.Success(ToDto(entry));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return OperationResult<ScoreEntryDto>.Failure("validation_error", ex.Message);
        }
    }

    private async Task<Guid?> ResolveCompetitionIdForRunAsync(Guid runId, CancellationToken cancellationToken)
    {
        return await (
                from run in dbContext.Runs.AsNoTracking()
                join heat in dbContext.Heats.AsNoTracking() on run.HeatId equals heat.Id
                where run.Id == runId
                select (Guid?)heat.CompetitionId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task TryPublishScoreRegisteredAsync(
        Guid competitionId,
        RegisterScoreRequest request)
    {
        try
        {
            var leaderboardAfter = await leaderboardQueryService.GetCompetitionLeaderboardAsync(
                new GetCompetitionLeaderboardRequest(competitionId),
                CancellationToken.None);

            var leaderboardBefore = ReconstructLeaderboardBeforeScoreChange(
                leaderboardAfter,
                request.ParticipantId,
                request.Rings);

            await realtimePublisher.PublishScoreRegisteredAsync(
                new ScoreRegisteredRealtimeEvent(
                    competitionId,
                    request.RunId,
                    request.ParticipantId,
                    request.Rings,
                    leaderboardAfter,
                    ResolveRankChange(leaderboardBefore, leaderboardAfter, request.ParticipantId)),
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Realtime publish failed after score registration. CompetitionId: {CompetitionId}, RunId: {RunId}, ParticipantId: {ParticipantId}",
                competitionId,
                request.RunId,
                request.ParticipantId);
        }
    }

    private async Task TryPublishScoreCorrectedAsync(
        Guid competitionId,
        CorrectScoreRequest request,
        int previousRings)
    {
        try
        {
            var leaderboardAfter = await leaderboardQueryService.GetCompetitionLeaderboardAsync(
                new GetCompetitionLeaderboardRequest(competitionId),
                CancellationToken.None);

            var leaderboardBefore = ReconstructLeaderboardBeforeScoreChange(
                leaderboardAfter,
                request.ParticipantId,
                request.Rings - previousRings);

            await realtimePublisher.PublishScoreCorrectedAsync(
                new ScoreCorrectedRealtimeEvent(
                    competitionId,
                    request.RunId,
                    request.ParticipantId,
                    previousRings,
                    request.Rings,
                    leaderboardAfter,
                    ResolveRankChange(leaderboardBefore, leaderboardAfter, request.ParticipantId)),
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Realtime publish failed after score correction. CompetitionId: {CompetitionId}, RunId: {RunId}, ParticipantId: {ParticipantId}",
                competitionId,
                request.RunId,
                request.ParticipantId);
        }
    }

    private static ParticipantRankChangedEvent? ResolveRankChange(
        CompetitionLeaderboardDto before,
        CompetitionLeaderboardDto after,
        Guid participantId)
    {
        var beforeRank = before.Rows.FirstOrDefault(x => x.ParticipantId == participantId)?.Rank;
        var afterRank = after.Rows.FirstOrDefault(x => x.ParticipantId == participantId)?.Rank;

        if (beforeRank is null || afterRank is null || beforeRank.Value == afterRank.Value)
        {
            return null;
        }

        return new ParticipantRankChangedEvent(participantId, beforeRank.Value, afterRank.Value);
    }

    private static CompetitionLeaderboardDto ReconstructLeaderboardBeforeScoreChange(
        CompetitionLeaderboardDto leaderboardAfter,
        Guid participantId,
        int ringsDelta)
    {
        var rowsBefore = leaderboardAfter.Rows
            .Select(row => row.ParticipantId == participantId
                ? row with { TotalRings = Math.Max(0, row.TotalRings - ringsDelta) }
                : row)
            .OrderByDescending(row => row.TotalRings)
            .ThenBy(row => row.ParticipantNumber)
            .Select((row, index) => row with { Rank = index + 1 })
            .ToList();

        return leaderboardAfter with { Rows = rowsBefore };
    }

    private static ScoreEntryDto ToDto(ScoreEntry entry) =>
        new(entry.Id, entry.RunId, entry.ParticipantId, entry.Rings, entry.RegisteredAtUtc);
}
