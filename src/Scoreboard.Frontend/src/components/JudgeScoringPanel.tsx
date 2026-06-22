import type { CompetitionRun, PendingScoreSubmission } from '../types';

type JudgeScoringPanelProps = {
  run: CompetitionRun | null;
  onRegisterScore: (participantId: string, rings: 0 | 1 | 2) => Promise<void>;
  pendingScores: PendingScoreSubmission[];
};

const ringValues: Array<0 | 1 | 2> = [0, 1, 2];

export function JudgeScoringPanel({ run, onRegisterScore, pendingScores }: JudgeScoringPanelProps) {
  if (!run) {
    return <p className="hint">Select a run to start scoring.</p>;
  }

  if (run.participants.length === 0) {
    return <p className="hint">This run has no participants assigned.</p>;
  }

  return (
    <div className="participant-list" aria-label="Run participants">
      {run.participants.map((participant) => {
        const participantPendingScores = pendingScores.filter(
          (submission) => submission.runId === run.runId && submission.participantId === participant.participantId
        );

        return (
          <article key={participant.participantId} className="participant-card">
            <div>
              <p className="participant-number">#{participant.participantNumber}</p>
              <h3>{participant.participantName}</h3>
              {participantPendingScores.length > 0 && (
                <p className="pending-note">
                  Pending sync: {participantPendingScores.map((submission) => submission.rings).join(', ')} rings
                </p>
              )}
            </div>
            <div className="rings-actions">
              {ringValues.map((rings) => (
                <button key={rings} type="button" onClick={() => onRegisterScore(participant.participantId, rings)}>
                  {rings}
                </button>
              ))}
            </div>
          </article>
        );
      })}
    </div>
  );
}
