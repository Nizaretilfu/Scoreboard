export type Competition = {
  id: string;
  name: string;
  competitionDate: string;
};

export type RunParticipant = {
  participantId: string;
  participantNumber: number;
  participantName: string;
};

export type CompetitionRun = {
  runId: string;
  heatId: string;
  heatSequenceNumber: number;
  runSequenceNumber: number;
  participants: RunParticipant[];
};

export type LeaderboardRow = {
  rank: number;
  participantId: string;
  participantNumber: number;
  participantName: string;
  totalRings: number;
};

export type CompetitionLeaderboard = {
  competitionId: string;
  rows: LeaderboardRow[];
  generatedAtUtc: string;
};


export type PendingScoreSubmission = {
  clientSubmissionId: string;
  runId: string;
  participantId: string;
  rings: 0 | 1 | 2;
  createdAtUtc: string;
  attemptCount: number;
  lastError?: string;
};
