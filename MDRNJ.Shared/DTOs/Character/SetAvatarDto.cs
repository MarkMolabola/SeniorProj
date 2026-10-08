using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Character
{
    public class SetAvatarDto
    {
        /// <summary>One of <see cref="MDRNJ.Shared.Game.HeroAvatars.All"/>.</summary>
        [Required]
        [MaxLength(40)]
        public required string AvatarId { get; set; }
    }
}
