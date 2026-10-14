using System.ComponentModel.DataAnnotations;
using MDRNJ.Shared.Game;

namespace MDRNJ.Shared.DTOs.Chat
{
    public class SendChatMessageDto
    {
        [Required]
        [MaxLength(ChatRules.MaxMessageLength)]
        public string Body { get; set; } = string.Empty;
    }
}
