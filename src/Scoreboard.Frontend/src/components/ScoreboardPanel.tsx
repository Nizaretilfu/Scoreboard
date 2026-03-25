import type { LeaderboardRow } from '../types';

type ScoreboardPanelProps = {
  rows: LeaderboardRow[];
};

export function ScoreboardPanel({ rows }: ScoreboardPanelProps) {
  if (rows.length === 0) {
    return <p className="hint">No scores yet.</p>;
  }

  return (
    <table className="scoreboard-table" aria-label="Live scoreboard">
      <thead>
        <tr>
          <th>Rank</th>
          <th>No.</th>
          <th>Participant</th>
          <th>Rings</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row) => (
          <tr key={row.participantId}>
            <td>{row.rank}</td>
            <td>{row.participantNumber}</td>
            <td>{row.participantName}</td>
            <td>{row.totalRings}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
