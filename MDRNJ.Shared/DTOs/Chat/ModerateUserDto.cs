using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Chat
{
    /// <summary>Admins: keep a player out of chat for a while. 0 hours lifts the mute.</summary>
    public class MuteUserDto
    {
        [Range(0, 24 * 30)]
        public int Hours { get; set; }
    }

    /// <summary>Admins: suspend (or restore) a player's account.</summary>
    public class BanUserDto
    {
        public bool Banned { get; set; }
    }
}
