using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MDRNJ.Api.Hubs
{
    [Authorize]
    public class LeaderboardHub : Hub
    {
        public async Task JoinLeaderboard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "leaderboard");
        }

        public async Task LeaveLeaderboard()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "leaderboard");
        }
    }
}
