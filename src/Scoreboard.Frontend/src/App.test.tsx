import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
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

  it('loads competitions and run participants', async () => {
    render(<App />);

    expect(await screen.findByText('Rider One')).toBeInTheDocument();
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
      expect(mockApi.registerScore).toHaveBeenCalledWith('r1', 'p1', 2);
    });
  });
});
