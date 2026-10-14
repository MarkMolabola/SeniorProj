using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Hubs
{
    /// <summary>
    /// Live chat. Messages are sent over REST (ChatController); this hub pushes them:
    ///   "MessageReceived" (ChatMessageDto) to everyone viewing that chat,
    ///   "ChannelActivity" (channelId) to every member of a group/direct chat, so chat lists refresh.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chat;

        public ChatHub(IChatService chat) => _chat = chat;

        public static string ChannelGroup(Guid channelId) => $"chat:{channelId}";

        public async Task JoinChannel(Guid channelId)
        {
            var userId = Guid.Parse(Context.UserIdentifier!);
            if (!await _chat.CanAccessAsync(userId, channelId))
                throw new HubException("Chat not found.");
            await Groups.AddToGroupAsync(Context.ConnectionId, ChannelGroup(channelId));
        }

        public Task LeaveChannel(Guid channelId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, ChannelGroup(channelId));
    }
}
