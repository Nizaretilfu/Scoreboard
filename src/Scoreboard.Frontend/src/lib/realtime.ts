import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr';
import { apiBaseUrl } from './api';

export const realtimeEvents = {
  scoreRegistered: 'scoreRegistered',
  scoreCorrected: 'scoreCorrected',
  leaderboardUpdated: 'leaderboardUpdated',
  participantRankChanged: 'participantRankChanged'
} as const;

export async function connectToLeaderboardHub(
  competitionId: string,
  onAnyLeaderboardEvent: () => void
): Promise<HubConnection> {
  const connection = new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/leaderboard`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  Object.values(realtimeEvents).forEach((eventName) => {
    connection.on(eventName, onAnyLeaderboardEvent);
  });

  if (connection.state === HubConnectionState.Disconnected) {
    await connection.start();
  }

  await connection.invoke('JoinCompetition', competitionId);

  return connection;
}
