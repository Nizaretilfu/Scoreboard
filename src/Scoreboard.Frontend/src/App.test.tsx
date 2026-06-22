import { cleanup, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { App } from './App';

const mockApi = vi.hoisted(() => ({
  getCompetitions: vi.fn(),
  getCompetitionRuns: vi.fn(),
  getLeaderboard: vi.fn(),
  registerScore: vi.fn()
}));

const mockRealtime = vi.hoisted(() => ({
  connectToLeaderboardHub: vi.fn().mockResolvedValue({ stop: vi.fn() })
}));

vi.mock('./lib/api', () => mockApi);
vi.mock('./lib/realtime', () => mockRealtime);

describe('App', () => {
  beforeEach(() => {
    cleanup();
    window.localStorage.clear();
    vi.clearAllMocks();
    mockApi.getCompetitions.mockResolvedValue([
      { id: 'c1', name: 'Spring Cup', competitionDate: '2026-03-20' }
    ]);
    mockApi.getCompetitionRuns.mockResolvedValue([
      {
        runId: 'r1',
        heatId: 'h1',
        heatSequenceNumber: 1,
        runSequenceNumber: 1,
        participants: [
          {
            participantId: 'p1',
            participantNumber: 12,
            participantName: 'Rider One'
          }
        ]
      }
    ]);
    mockApi.getLeaderboard.mockResolvedValue({
      competitionId: 'c1',
      generatedAtUtc: '2026-03-20T00:00:00Z',
      rows: [
        {
          rank: 1,
          participantId: 'p1',
          participantNumber: 12,
          participantName: 'Rider One',
          totalRings: 2
        }
      ]
    });
    mockApi.registerScore.mockResolvedValue({});
  });

  afterEach(() => {
    cleanup();
    window.localStorage.clear();
  });

  it('loads competitions and run participants', async () => {
    render(<App />);

    expect((await screen.findAllByText('Rider One')).length).toBeGreaterThan(0);
    expect(mockApi.getCompetitions).toHaveBeenCalledTimes(1);
    expect(mockApi.getCompetitionRuns).toHaveBeenCalledWith('c1');
    expect(mockRealtime.connectToLeaderboardHub).toHaveBeenCalledWith('c1', expect.any(Function));
  });

  it('registers score with one tap flow', async () => {
    const user = userEvent.setup();
    render(<App />);

    const scoreButton = await screen.findByRole('button', { name: '2' });
    await user.click(scoreButton);

    await waitFor(() => {
      expect(mockApi.registerScore).toHaveBeenCalledWith('r1', 'p1', 2, expect.any(String));
    });
  });

  it('stops stale SignalR connection when competition changes before connect resolves', async () => {
    const user = userEvent.setup();
    let resolveConnection: ((value: { stop: ReturnType<typeof vi.fn> }) => void) | null = null;
    const staleStop = vi.fn().mockResolvedValue(undefined);

    mockApi.getCompetitions.mockResolvedValue([
      { id: 'c1', name: 'Spring Cup', competitionDate: '2026-03-20' },
      { id: 'c2', name: 'Summer Cup', competitionDate: '2026-03-21' }
    ]);
    mockApi.getCompetitionRuns.mockResolvedValue([]);
    mockRealtime.connectToLeaderboardHub.mockImplementationOnce(
      () =>
        new Promise((resolve) => {
          resolveConnection = resolve;
        })
    );
    mockRealtime.connectToLeaderboardHub.mockResolvedValue({ stop: vi.fn().mockResolvedValue(undefined) });

    render(<App />);

    await screen.findByText('Summer Cup');
    const competitionSelect = screen.getByLabelText('Competition');
    await user.selectOptions(competitionSelect, 'c2');

    expect(resolveConnection).not.toBeNull();
    resolveConnection!({ stop: staleStop });

    await waitFor(() => {
      expect(staleStop).toHaveBeenCalledTimes(1);
    });
  });

  it('clears stale runs immediately when switching competitions', async () => {
    const user = userEvent.setup();
    mockApi.getCompetitions.mockResolvedValue([
      { id: 'c1', name: 'Spring Cup', competitionDate: '2026-03-20' },
      { id: 'c2', name: 'Summer Cup', competitionDate: '2026-03-21' }
    ]);
    mockApi.getCompetitionRuns.mockImplementation(async (competitionId: string) =>
      competitionId === 'c1'
        ? [
            {
              runId: 'r1',
              heatId: 'h1',
              heatSequenceNumber: 1,
              runSequenceNumber: 1,
              participants: []
            }
          ]
        : []
    );
    mockApi.getLeaderboard.mockResolvedValue({
      competitionId: 'c1',
      generatedAtUtc: '2026-03-20T00:00:00Z',
      rows: []
    });

    render(<App />);

    await screen.findByRole('option', { name: 'Heat 1 / Run 1' });
    const competitionSelect = screen.getByLabelText('Competition');
    await user.selectOptions(competitionSelect, 'c2');

    const runSelect = screen.getByLabelText('Run');
    expect(within(runSelect).queryByRole('option', { name: 'Heat 1 / Run 1' })).not.toBeInTheDocument();
  });

  it('keeps failed score submissions pending and retries automatically', async () => {
    const user = userEvent.setup();
    mockApi.registerScore.mockRejectedValueOnce(new Error('network down')).mockResolvedValueOnce({});

    render(<App />);

    const scoreButton = await screen.findByRole('button', { name: '2' });
    await user.click(scoreButton);

    expect(await screen.findByText(/1 score pending/i)).toBeInTheDocument();
    expect(await screen.findByText(/Pending sync: 2 rings/i)).toBeInTheDocument();

    await waitFor(() => {
      expect(mockApi.registerScore).toHaveBeenCalledTimes(2);
    }, { timeout: 4000 });

    expect(await screen.findByText(/All scores sent/i)).toBeInTheDocument();
  });

});
