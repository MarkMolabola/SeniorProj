using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.Enums
{
    public enum ChallengeType
    {
        Distance = 0,  // run/cycle X km
        Streak = 1,  // work out X days in a row
        FriendDuel = 2, // head to head vs a friend
        XPRace = 3,  // earn the most XP in a time window
        WorkoutCount = 4 // complete X workouts in a period
    }
}
