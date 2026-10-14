namespace MDRNJ.Shared.DTOs.Chat
{
    public class ChatMemberDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
    }
}
