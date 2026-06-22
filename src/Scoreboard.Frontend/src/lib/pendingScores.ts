import type { PendingScoreSubmission } from '../types';

const storageKey = 'scoreboard.pendingScoreSubmissions.v1';

export function loadPendingScores(): PendingScoreSubmission[] {
  try {
    const raw = window.localStorage.getItem(storageKey);
    if (!raw) {
      return [];
    }

    const parsed = JSON.parse(raw) as PendingScoreSubmission[];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export function savePendingScores(submissions: PendingScoreSubmission[]) {
  window.localStorage.setItem(storageKey, JSON.stringify(submissions));
}

export function createPendingScore(runId: string, participantId: string, rings: 0 | 1 | 2): PendingScoreSubmission {
  return {
    clientSubmissionId: crypto.randomUUID(),
    runId,
    participantId,
    rings,
    createdAtUtc: new Date().toISOString(),
    attemptCount: 0
  };
}
