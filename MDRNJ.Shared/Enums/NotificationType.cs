using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.Enums
{
    public enum NotificationType
    {
        StreakReminder = 0,  // user hasn't worked out today
        AchievementUnlocked = 1,  // badge earned
        ChallengeInvite = 2,  // friend sent a challenge
        ChallengeCompleted = 3,  // challenge finished
        FriendRequest = 4,  // someone added you
        CharacterEvolved = 5,  // character changed appearance
        LevelUp = 6,  // user reached a new level
        FriendActivity = 7,  // friend completed a workout
        FriendAccepted = 8,  // your friend request was accepted
        ChallengeAccepted = 9,  // a friend accepted your challenge
        ChallengeDeclined = 10, // a friend declined your challenge
        ChallengeReview = 11,   // results are in: check your opponent's proof
        ChallengeDisputed = 12  // your opponent disputed your proof
    }
}
