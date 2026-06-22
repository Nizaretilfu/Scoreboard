import { useEffect, useMemo, useState } from 'react';
import { getCompetitionRuns, getCompetitions, getLeaderboard, registerScore } from './lib/api';
import { createPendingScore, loadPendingScores, savePendingScores } from './lib/pendingScores';
import { connectToLeaderboardHub } from './lib/realtime';
import { JudgeScoringPanel } from './components/JudgeScoringPanel';
import { ScoreboardPanel } from './components/ScoreboardPanel';
import type { Competition, CompetitionRun, LeaderboardRow, PendingScoreSubmission } from './types';

export function App() {
  const [competitions, setCompetitions] = useState<Competition[]>([]);
  const [selectedCompetitionId, setSelectedCompetitionId] = useState<string>('');
  const [runs, setRuns] = useState<CompetitionRun[]>([]);
  const [selectedRunId, setSelectedRunId] = useState<string>('');
  const [rows, setRows] = useState<LeaderboardRow[]>([]);
  const [status, setStatus] = useState<string>('Loading competitions...');
  const [pendingScores, setPendingScores] = useState<PendingScoreSubmission[]>(() => loadPendingScores());
  const [isOnline, setIsOnline] = useState(() => navigator.onLine);

  const selectedRun = useMemo(
    () => runs.find((run) => run.runId === selectedRunId) ?? null,
    [runs, selectedRunId]
  );

  useEffect(() => {
    savePendingScores(pendingScores);
  }, [pendingScores]);

  useEffect(() => {
    const updateOnlineStatus = () => setIsOnline(navigator.onLine);

    window.addEventListener('online', updateOnlineStatus);
    window.addEventListener('offline', updateOnlineStatus);

    return () => {
      window.removeEventListener('online', updateOnlineStatus);
      window.removeEventListener('offline', updateOnlineStatus);
    };
  }, []);

  useEffect(() => {
    if (!isOnline || pendingScores.length === 0) {
      return;
    }

    let isCancelled = false;

    const syncPendingScores = async () => {
      const [nextSubmission] = pendingScores;

      try {
        await registerScore(
          nextSubmission.runId,
          nextSubmission.participantId,
          nextSubmission.rings,
          nextSubmission.clientSubmissionId
        );

        if (!isCancelled) {
          setPendingScores((current) =>
            current.filter((submission) => submission.clientSubmissionId !== nextSubmission.clientSubmissionId)
          );
          setStatus('Scores synced.');
        }
      } catch (error) {
        if (!isCancelled) {
          setPendingScores((current) =>
            current.map((submission) =>
              submission.clientSubmissionId === nextSubmission.clientSubmissionId
                ? {
                    ...submission,
                    attemptCount: submission.attemptCount + 1,
                    lastError: error instanceof Error ? error.message : 'Sync failed'
                  }
                : submission
            )
          );
          setStatus('Score pending. Will retry automatically.');
        }
      }
    };

    const retryDelayMs = pendingScores[0].attemptCount === 0 ? 0 : 3000;
    const timeout = window.setTimeout(() => {
      void syncPendingScores();
    }, retryDelayMs);

    return () => {
      isCancelled = true;
      window.clearTimeout(timeout);
    };
  }, [isOnline, pendingScores]);

  useEffect(() => {
    getCompetitions()
      .then((items) => {
        setCompetitions(items);
        if (items.length > 0) {
          setSelectedCompetitionId(items[0].id);
          setStatus('Select a run and register scores.');
        } else {
          setStatus('No competitions found.');
        }
      })
      .catch(() => setStatus('Could not load competitions.'));
  }, []);

  useEffect(() => {
    if (!selectedCompetitionId) {
      setRuns([]);
      setSelectedRunId('');
      setRows([]);
      return;
    }

    setRuns([]);
    setSelectedRunId('');

    let isCancelled = false;
    let stopConnection: (() => Promise<void>) | null = null;

    const refreshLeaderboard = async () => {
      const leaderboard = await getLeaderboard(selectedCompetitionId);
      if (!isCancelled) {
        setRows(leaderboard.rows);
      }
    };

    getCompetitionRuns(selectedCompetitionId)
      .then((loadedRuns) => {
        if (isCancelled) {
          return;
        }

        setRuns(loadedRuns);
        setSelectedRunId((current) => current || loadedRuns[0]?.runId || '');
      })
      .catch(() => {
        if (!isCancelled) {
          setStatus('Could not load runs.');
        }
      });

    refreshLeaderboard().catch(() => {
      if (!isCancelled) {
        setStatus('Could not load leaderboard.');
      }
    });

    connectToLeaderboardHub(selectedCompetitionId, () => {
      refreshLeaderboard().catch(() => {
        if (!isCancelled) {
          setStatus('Realtime update failed, refresh to retry.');
        }
      });
    })
      .then((connection) => {
        if (isCancelled) {
          void connection.stop();
          return;
        }

        stopConnection = () => connection.stop();
      })
      .catch(() => {
        if (!isCancelled) {
          setStatus('Could not connect to live updates.');
        }
      });

    return () => {
      isCancelled = true;
      void stopConnection?.();
    };
  }, [selectedCompetitionId]);

  async function handleRegisterScore(participantId: string, rings: 0 | 1 | 2) {
    if (!selectedRunId) {
      return;
    }

    const submission = createPendingScore(selectedRunId, participantId, rings);
    setPendingScores((current) => [...current, submission]);
    setStatus(`Queued ${rings} rings. Syncing...`);
  }

  return (
    <main className="app-shell">
      <h1>Judge Scoring</h1>
      <p className="hint">{status}</p>
      <p className={pendingScores.length > 0 ? 'sync-status sync-status--pending' : 'sync-status'} role="status">
        {isOnline ? 'Online' : 'Offline'} ·{' '}
        {pendingScores.length === 0
          ? 'All scores sent'
          : `${pendingScores.length} score${pendingScores.length === 1 ? '' : 's'} pending`}
      </p>

      <section className="selectors">
        <label>
          Competition
          <select
            value={selectedCompetitionId}
            onChange={(event) => {
              setRuns([]);
              setSelectedCompetitionId(event.target.value);
              setSelectedRunId('');
            }}>
            <option value="">Select competition</option>
            {competitions.map((competition) => (
              <option key={competition.id} value={competition.id}>
                {competition.name}
              </option>
            ))}
          </select>
        </label>

        <label>
          Run
          <select value={selectedRunId} onChange={(event) => setSelectedRunId(event.target.value)}>
            <option value="">Select run</option>
            {runs.map((run) => (
              <option key={run.runId} value={run.runId}>
                Heat {run.heatSequenceNumber} / Run {run.runSequenceNumber}
              </option>
            ))}
          </select>
        </label>
      </section>

      <JudgeScoringPanel run={selectedRun} onRegisterScore={handleRegisterScore} pendingScores={pendingScores} />

      <section>
        <h2>Live scoreboard</h2>
        <ScoreboardPanel rows={rows} />
      </section>
    </main>
  );
}
