namespace MDRNJ.Shared.Enums
{
    public enum ChallengeStatus
    {
        Pending = 0,
        Active = 1,
        Completed = 2,
        Expired = 3,
        Cancelled = 4,
        Declined = 5,

        /// <summary>Results are in; players have a window to check each other's proof.</summary>
        InReview = 6,

        /// <summary>A player disputed the other's proof: void, no winner and no rewards.</summary>
        Disputed = 7
    }
}
