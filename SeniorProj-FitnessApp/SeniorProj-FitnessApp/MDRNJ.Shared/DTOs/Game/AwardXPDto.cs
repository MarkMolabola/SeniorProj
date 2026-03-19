using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MDRNJ.Shared.DTOs.Game
{
    public class AwardXPDto
    {
        [Required]
        public required Guid UserId { get; set; }

        [Required]
        [Range(1, 10000)]
        public required int Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Reason { get; set; }

        public Guid? SourceId { get; set; }
    }
}
