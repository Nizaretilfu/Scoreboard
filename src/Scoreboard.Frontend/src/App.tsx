import { useEffect, useMemo, useState } from 'react';
import { getCompetitionRuns, getCompetitions, getLeaderboard, registerScore } from './lib/api';
import { connectToLeaderboardHub } from './lib/realtime';
import { JudgeScoringPanel } from './components/JudgeScoringPanel';
import { ScoreboardPanel } from './components/ScoreboardPanel';
import type { Competition, CompetitionRun, LeaderboardRow } from './types';

export function App() {
  const [competitions, setCompetitions] = useState<Competition[]>([]);
  const [selectedCompetitionId, setSelectedCompetitionId] = useState<string>('');
  const [runs, setRuns] = useState<CompetitionRun[]>([]);
  const [selectedRunId, setSelectedRunId] = useState<string>('');
  const [rows, setRows] = useState<LeaderboardRow[]>([]);
  const [status, setStatus] = useState<string>('Loading competitions...');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const selectedRun = useMemo(
    () => runs.find((run) => run.runId === selectedRunId) ?? null,
    [runs, selectedRunId]
  );

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
      setRows([]);
      return;
    }

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

    setIsSubmitting(true);
    try {
      await registerScore(selectedRunId, participantId, rings);
      setStatus(`Registered ${rings} rings.`);
    } catch {
      setStatus('Failed to register score.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="app-shell">
      <h1>Judge Scoring</h1>
      <p className="hint">{status}</p>

      <section className="selectors">
        <label>
          Competition
          <select
            value={selectedCompetitionId}
            onChange={(event) => {
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

      <JudgeScoringPanel run={selectedRun} onRegisterScore={handleRegisterScore} isSubmitting={isSubmitting} />

      <section>
        <h2>Live scoreboard</h2>
        <ScoreboardPanel rows={rows} />
      </section>
    </main>
  );
}
