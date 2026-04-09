using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.Enums;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class CharacterClassifierService
    {
        // TODO: inject HttpClient and IConfiguration for LLM API calls
        public CharacterClassifierService() { }

        public async Task<BodyType> ClassifyBodyTypeAsync(ActivityScoreEntity activityScore)
        {
            // TODO: call LLM API with activityScore data and parse response
            await Task.CompletedTask;

            if (activityScore.StrengthScore >= 60)
                return activityScore.StrengthScore >= 80 ? BodyType.Powerlifter : BodyType.Muscular;

            if (activityScore.CardioScore >= 60)
                return BodyType.Slim;

            if (activityScore.StrengthScore >= 30 && activityScore.CardioScore >= 30)
                return BodyType.Athletic;

            return BodyType.Toned;
        }
    }
}
