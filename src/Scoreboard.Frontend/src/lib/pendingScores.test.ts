import { afterEach, describe, expect, it, vi } from 'vitest';
import { createPendingScore, loadPendingScores, savePendingScores } from './pendingScores';

describe('pending score storage', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.restoreAllMocks();
  });

  it('persists pending score submissions locally', () => {
    const submission = createPendingScore('r1', 'p1', 2);

    savePendingScores([submission]);

    expect(loadPendingScores()).toEqual([submission]);
  });

  it('ignores corrupt storage instead of blocking judge scoring', () => {
    window.localStorage.setItem('scoreboard.pendingScoreSubmissions.v1', 'not json');

    expect(loadPendingScores()).toEqual([]);
  });

  it('creates a stable client submission id for idempotent retries', () => {
    vi.spyOn(crypto, 'randomUUID').mockReturnValue('11111111-1111-4111-8111-111111111111');

    expect(createPendingScore('r1', 'p1', 1)).toMatchObject({
      clientSubmissionId: '11111111-1111-4111-8111-111111111111',
      runId: 'r1',
      participantId: 'p1',
      rings: 1,
      attemptCount: 0
    });
  });

  it('creates different submission ids for the same participant across different runs', () => {
    vi.spyOn(crypto, 'randomUUID')
      .mockReturnValueOnce('11111111-1111-4111-8111-111111111111')
      .mockReturnValueOnce('22222222-2222-4222-8222-222222222222');

    const firstRunSubmission = createPendingScore('run-1', 'participant-1', 2);
    const secondRunSubmission = createPendingScore('run-2', 'participant-1', 1);

    expect(firstRunSubmission.clientSubmissionId).not.toEqual(secondRunSubmission.clientSubmissionId);
    expect(firstRunSubmission.runId).toBe('run-1');
    expect(secondRunSubmission.runId).toBe('run-2');
  });
});
