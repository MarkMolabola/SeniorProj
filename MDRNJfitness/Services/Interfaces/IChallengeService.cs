using MDRNJfitness.Models.Social;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IChallengeService
    {
        List<Challenge> GetOrCreateWeeklyChallenges(string difficulty);
        void ResetWeeklyChallengesForTesting();
    }
}
