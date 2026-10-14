namespace MDRNJ.Shared.DTOs.Chat
{
    public class AddChatMembersDto
    {
        public List<Guid> UserIds { get; set; } = new();
    }
}
