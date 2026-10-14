using MDRNJ.Shared.DTOs.Chat;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _http;

        public ChatService(HttpClient http) => _http = http;

        public Task<List<ChatChannelDto>> GetChannelsAsync() =>
            _http.GetAsync<List<ChatChannelDto>>("api/chat/channels")!;

        public Task<List<ChatMessageDto>> GetMessagesAsync(Guid channelId, DateTime? before = null)
        {
            var url = $"api/chat/channels/{channelId}/messages";
            if (before is DateTime b)
            {
                // The API stores UTC without a zone; send it back the same way so it isn't shifted
                var utc = b.Kind == DateTimeKind.Local ? b.ToUniversalTime() : b;
                url += "?before=" + Uri.EscapeDataString(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"));
            }
            return _http.GetAsync<List<ChatMessageDto>>(url)!;
        }

        public Task<ChatMessageDto> SendMessageAsync(Guid channelId, string body) =>
            _http.PostAsync<ChatMessageDto>($"api/chat/channels/{channelId}/messages", new SendChatMessageDto { Body = body })!;

        public Task MarkReadAsync(Guid channelId) =>
            _http.PostAsync($"api/chat/channels/{channelId}/read", new { });

        public Task<List<ChatMemberDto>> GetMembersAsync(Guid channelId) =>
            _http.GetAsync<List<ChatMemberDto>>($"api/chat/channels/{channelId}/members")!;

        public Task<ChatChannelDto> OpenDirectAsync(Guid otherUserId) =>
            _http.PostAsync<ChatChannelDto>($"api/chat/direct/{otherUserId}", new { })!;

        public Task<ChatChannelDto> CreateGroupAsync(string name, List<Guid> memberIds) =>
            _http.PostAsync<ChatChannelDto>("api/chat/groups", new CreateChatGroupDto { Name = name, MemberIds = memberIds })!;

        public Task<List<ChatMemberDto>> AddMembersAsync(Guid channelId, List<Guid> userIds) =>
            _http.PostAsync<List<ChatMemberDto>>($"api/chat/groups/{channelId}/members", new AddChatMembersDto { UserIds = userIds })!;

        public Task LeaveGroupAsync(Guid channelId) =>
            _http.PostAsync($"api/chat/groups/{channelId}/leave", new { });
    }
}
