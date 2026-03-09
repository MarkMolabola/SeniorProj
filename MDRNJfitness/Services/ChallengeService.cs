using System.Text.Json;
using MDRNJfitness.Models.Social;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class ChallengeService : IChallengeService
    {
        private readonly Random _random = new();

        private const string WeeklyChallengesKey = "weekly_challenges";
        private const string WeeklyDifficultyKey = "weekly_difficulty";
        private const string WeeklyStartKey = "weekly_start";

        public List<Challenge> GetOrCreateWeeklyChallenges(string difficulty)
        {
            var currentWeekStart = GetCurrentWeeklyResetPoint(DateTime.Now);

            var savedWeekStartString = Preferences.Default.Get(WeeklyStartKey, string.Empty);
            var savedChallengesJson = Preferences.Default.Get(WeeklyChallengesKey, string.Empty);

            if (!string.IsNullOrWhiteSpace(savedWeekStartString) &&
                !string.IsNullOrWhiteSpace(savedChallengesJson) &&
                DateTime.TryParse(savedWeekStartString, out var savedWeekStart) &&
                savedWeekStart == currentWeekStart)
            {
                var savedChallenges = JsonSerializer.Deserialize<List<Challenge>>(savedChallengesJson);
                if (savedChallenges != null && savedChallenges.Count > 0)
                {
                    return savedChallenges;
                }
            }

            var newChallenges = GenerateWeeklyChallenges(difficulty, currentWeekStart);

            Preferences.Default.Set(WeeklyStartKey, currentWeekStart.ToString("O"));
            Preferences.Default.Set(WeeklyDifficultyKey, difficulty);

            var json = JsonSerializer.Serialize(newChallenges);
            Preferences.Default.Set(WeeklyChallengesKey, json);

            return newChallenges;
        }

        public void ResetWeeklyChallengesForTesting()
        {
            Preferences.Default.Remove(WeeklyChallengesKey);
            Preferences.Default.Remove(WeeklyDifficultyKey);
            Preferences.Default.Remove(WeeklyStartKey);
        }

        private List<Challenge> GenerateWeeklyChallenges(string difficulty, DateTime weekStart)
        {
            difficulty = difficulty?.Trim() ?? "Easy";

            List<Challenge> challengePool;

            if (difficulty.Equals("Easy", StringComparison.OrdinalIgnoreCase))
            {
                int easySteps = _random.Next(12000, 18001);
                int easyMinutes = _random.Next(60, 121);

                challengePool = new List<Challenge>
                {
                    new Challenge
                    {
                        Title = "Workout Starter",
                        Description = "Complete 2 workouts this week.",
                        GoalType = "TotalWorkouts",
                        TargetValue = 2,
                        CurrentProgress = 0,
                        XpReward = 100,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Cardio Kickoff",
                        Description = "Complete 1 cardio workout this week.",
                        GoalType = "CardioSessions",
                        TargetValue = 1,
                        CurrentProgress = 0,
                        XpReward = 75,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Leg Day Intro",
                        Description = "Complete 1 leg workout this week.",
                        GoalType = "LegSessions",
                        TargetValue = 1,
                        CurrentProgress = 0,
                        XpReward = 75,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Upper Body Intro",
                        Description = "Complete 1 upper body workout this week.",
                        GoalType = "UpperBodySessions",
                        TargetValue = 1,
                        CurrentProgress = 0,
                        XpReward = 75,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Step Into Action",
                        Description = $"Reach {easySteps} total steps this week.",
                        GoalType = "Steps",
                        TargetValue = easySteps,
                        CurrentProgress = 0,
                        XpReward = 50,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Active Minutes",
                        Description = $"Reach {easyMinutes} active minutes this week.",
                        GoalType = "WorkoutMinutes",
                        TargetValue = easyMinutes,
                        CurrentProgress = 0,
                        XpReward = 60,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    }
                };
            }
            else if (difficulty.Equals("Medium", StringComparison.OrdinalIgnoreCase))
            {
                int mediumSteps = _random.Next(25000, 35001);
                int mediumMinutes = _random.Next(120, 221);

                challengePool = new List<Challenge>
                {
                    new Challenge
                    {
                        Title = "Workout Warrior",
                        Description = "Complete 4 workouts this week.",
                        GoalType = "TotalWorkouts",
                        TargetValue = 4,
                        CurrentProgress = 0,
                        XpReward = 200,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Cardio Builder",
                        Description = "Complete 2 cardio workouts this week.",
                        GoalType = "CardioSessions",
                        TargetValue = 2,
                        CurrentProgress = 0,
                        XpReward = 125,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Leg Focus",
                        Description = "Complete 2 leg workouts this week.",
                        GoalType = "LegSessions",
                        TargetValue = 2,
                        CurrentProgress = 0,
                        XpReward = 125,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Upper Body Focus",
                        Description = "Complete 2 upper body workouts this week.",
                        GoalType = "UpperBodySessions",
                        TargetValue = 2,
                        CurrentProgress = 0,
                        XpReward = 125,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Balanced Training",
                        Description = "Complete 1 leg workout and 1 upper body workout this week.",
                        GoalType = "SplitSessions",
                        TargetValue = 2,
                        CurrentProgress = 0,
                        XpReward = 150,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Active Week",
                        Description = $"Reach {mediumSteps} total steps this week.",
                        GoalType = "Steps",
                        TargetValue = mediumSteps,
                        CurrentProgress = 0,
                        XpReward = 100,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Consistency Minutes",
                        Description = $"Reach {mediumMinutes} active minutes this week.",
                        GoalType = "WorkoutMinutes",
                        TargetValue = mediumMinutes,
                        CurrentProgress = 0,
                        XpReward = 110,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    }
                };
            }
            else
            {
                int hardSteps = _random.Next(40000, 55001);
                int hardMinutes = _random.Next(180, 301);

                challengePool = new List<Challenge>
                {
                    new Challenge
                    {
                        Title = "Elite Consistency",
                        Description = "Complete 5 workouts this week.",
                        GoalType = "TotalWorkouts",
                        TargetValue = 5,
                        CurrentProgress = 0,
                        XpReward = 300,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Cardio Beast",
                        Description = "Complete 3 cardio workouts this week.",
                        GoalType = "CardioSessions",
                        TargetValue = 3,
                        CurrentProgress = 0,
                        XpReward = 200,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Leg Specialist",
                        Description = "Complete 2 leg workouts this week.",
                        GoalType = "LegSessions",
                        TargetValue = 2,
                        CurrentProgress = 0,
                        XpReward = 175,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Upper Body Specialist",
                        Description = "Complete 2 upper body workouts this week.",
                        GoalType = "UpperBodySessions",
                        TargetValue = 2,
                        CurrentProgress = 0,
                        XpReward = 175,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Training Specialist",
                        Description = "Complete 2 cardio workouts and 1 leg workout this week.",
                        GoalType = "FocusedSessions",
                        TargetValue = 3,
                        CurrentProgress = 0,
                        XpReward = 225,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Step Champion",
                        Description = $"Reach {hardSteps} total steps this week.",
                        GoalType = "Steps",
                        TargetValue = hardSteps,
                        CurrentProgress = 0,
                        XpReward = 150,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    },
                    new Challenge
                    {
                        Title = "Endurance Push",
                        Description = $"Reach {hardMinutes} active minutes this week.",
                        GoalType = "WorkoutMinutes",
                        TargetValue = hardMinutes,
                        CurrentProgress = 0,
                        XpReward = 160,
                        WeekStartDate = weekStart,
                        Difficulty = difficulty
                    }
                };
            }

            return challengePool
                .OrderBy(x => _random.Next())
                .Take(3)
                .ToList();
        }

        private DateTime GetCurrentWeeklyResetPoint(DateTime now)
        {
            int daysSinceMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            DateTime thisMondayNoon = now.Date.AddDays(-daysSinceMonday).AddHours(12);

            if (now < thisMondayNoon)
            {
                thisMondayNoon = thisMondayNoon.AddDays(-7);
            }

            return thisMondayNoon;
        }
    }
}
