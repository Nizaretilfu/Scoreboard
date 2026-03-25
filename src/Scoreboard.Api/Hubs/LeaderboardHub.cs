using Microsoft.AspNetCore.SignalR;

namespace Scoreboard.Api.Hubs;

public sealed class LeaderboardHub : Hub
{
    public Task JoinCompetition(Guid competitionId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GetCompetitionGroupName(competitionId));

    public Task LeaveCompetition(Guid competitionId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GetCompetitionGroupName(competitionId));

    public static string GetCompetitionGroupName(Guid competitionId) => $"competition:{competitionId:D}";
}
