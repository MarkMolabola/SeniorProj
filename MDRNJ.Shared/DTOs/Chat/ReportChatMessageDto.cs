using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Chat
{
    public class ReportChatMessageDto
    {
        /// <summary>One of MDRNJ.Shared.Game.ChatRules.ReportReasons.</summary>
        [Required]
        [MaxLength(40)]
        public string Reason { get; set; } = string.Empty;
    }
}
