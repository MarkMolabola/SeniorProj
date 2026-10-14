using System.ComponentModel.DataAnnotations;
using MDRNJ.Shared.Game;

namespace MDRNJ.Shared.DTOs.Chat
{
    public class CreateChatGroupDto
    {
        [Required]
        [MaxLength(ChatRules.MaxGroupNameLength)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Friends to add right away (the creator is always a member).</summary>
        public List<Guid> MemberIds { get; set; } = new();
    }
}
