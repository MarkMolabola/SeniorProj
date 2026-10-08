using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.Enums;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>Picks the hero's body type from the player's recent training mix.</summary>
    public class CharacterClassifierService
    {
        public Task<BodyType> ClassifyBodyTypeAsync(ActivityScoreEntity activityScore)
        {
            if (activityScore.StrengthScore >= 60)
                return Task.FromResult(activityScore.StrengthScore >= 80 ? BodyType.Powerlifter : BodyType.Muscular);

            if (activityScore.CardioScore >= 60)
                return Task.FromResult(BodyType.Slim);

            if (activityScore.StrengthScore >= 30 && activityScore.CardioScore >= 30)
                return Task.FromResult(BodyType.Athletic);

            return Task.FromResult(BodyType.Toned);
        }
    }
}
