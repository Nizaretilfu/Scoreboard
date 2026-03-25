import type { Competition, CompetitionLeaderboard, CompetitionRun } from '../types';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
const apiKey = import.meta.env.VITE_API_KEY ?? 'dev-scoreboard-key';

async function readJson<T>(input: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${input}`, init);
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  return response.json() as Promise<T>;
}

export function getCompetitions() {
  return readJson<Competition[]>('/api/setup/competitions');
}

export function getCompetitionRuns(competitionId: string) {
  return readJson<CompetitionRun[]>(`/api/setup/competitions/${competitionId}/runs`);
}

export function registerScore(runId: string, participantId: string, rings: 0 | 1 | 2) {
  return readJson('/api/scoring/scores', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Api-Key': apiKey
    },
    body: JSON.stringify({ runId, participantId, rings })
  });
}

export function getLeaderboard(competitionId: string) {
  return readJson<CompetitionLeaderboard>(`/api/leaderboard/competitions/${competitionId}`);
}

export { apiBaseUrl };
