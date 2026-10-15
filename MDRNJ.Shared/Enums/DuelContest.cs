namespace MDRNJ.Shared.Enums
{
    /// <summary>What a 1v1 friend challenge is about.</summary>
    public enum DuelContest
    {
        PushUps = 0,
        PullUps = 1,
        Plank = 2,
        Run = 3
    }

    /// <summary>What a player uploads as proof of a result.</summary>
    public enum ProofKind
    {
        /// <summary>A recording of the reps or the hold.</summary>
        Video = 0,

        /// <summary>A photo or screenshot of the run summary (treadmill, fitness app).</summary>
        Photo = 1
    }

    /// <summary>How a challenge's submissions turn into a score.</summary>
    public enum DuelScoring
    {
        /// <summary>One locked-in attempt each: max reps, longest hold, or fastest run over the set distance.</summary>
        BestAttempt = 0,

        /// <summary>Attempts add up until the deadline: total reps, plank seconds, or km.</summary>
        Total = 1
    }
}
