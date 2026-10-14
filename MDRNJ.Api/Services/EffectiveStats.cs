using MDRNJ.Api.Data;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// A hero's stat points as they count in play: trained points plus boosts from
    /// worn gear (GearStats). Used for stat-gated achievements and dungeon power.
    /// </summary>
    public static class EffectiveStats
    {
        public static async Task<(float Strength, float Endurance, float Flexibility)> LoadAsync(AppDbContext db, Guid userId)
        {
            var trained = await db.ActivityScores
                .Where(a => a.UserId == userId)
                .Select(a => new { a.StrengthPoints, a.CardioPoints, a.FlexibilityPoints })
                .FirstOrDefaultAsync();
            var character = await db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId);
            var gear = GearStats.Bonus(character);

            return ((trained?.StrengthPoints ?? 0) + gear.Strength,
                    (trained?.CardioPoints ?? 0) + gear.Endurance,
                    (trained?.FlexibilityPoints ?? 0) + gear.Flexibility);
        }
    }
}
