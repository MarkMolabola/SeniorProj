using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IAchievementService _achievements;
        private readonly ICharacterService _character;
        private readonly IGoalProgressService _goals;

        public WorkoutService(AppDbContext db, IMapper mapper, IAchievementService achievements,
            ICharacterService character, IGoalProgressService goals)
        {
            _db = db;
            _mapper = mapper;
            _achievements = achievements;
            _character = character;
            _goals = goals;
        }

        public async Task<WorkoutDto> StartWorkoutAsync(Guid userId, CreateWorkoutDto dto)
        {
            if (!Enum.TryParse<WorkoutType>(dto.WorkoutType, true, out var workoutType))
                throw new ArgumentException($"Unknown workout type: {dto.WorkoutType}");

            var session = new WorkoutSessionEntity
            {
                UserId = userId,
                WorkoutType = workoutType,
                StartTime = dto.StartTime,
                Notes = dto.Notes
            };

            _db.WorkoutSessions.Add(session);
            await _db.SaveChangesAsync();
            return _mapper.Map<WorkoutDto>(session);
        }

        public async Task<WorkoutDto> EndWorkoutAsync(Guid userId, Guid sessionId, float? distanceKm)
        {
            var session = await _db.WorkoutSessions
                .Include(s => s.Exercises)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId)
                ?? throw new KeyNotFoundException("Workout session not found.");

            if (session.EndTime.HasValue)
                throw new InvalidOperationException("Workout already ended.");

            session.EndTime = DateTime.UtcNow;
            session.DurationMins = (int)(session.EndTime.Value - session.StartTime).TotalMinutes;
            session.XPAwarded = Math.Max(10, session.DurationMins * 2);
            AddDistance(session, distanceKm);

            return await RecordCompletedAsync(userId, session);
        }

        public async Task<WorkoutDto> BacklogWorkoutAsync(Guid userId, BacklogWorkoutDto dto)
        {
            if (!Enum.TryParse<WorkoutType>(dto.WorkoutType, true, out var workoutType))
                throw new ArgumentException($"Unknown workout type: {dto.WorkoutType}");

            var startUtc = dto.StartTime.Kind == DateTimeKind.Utc
                ? dto.StartTime
                : dto.StartTime.ToUniversalTime();

            var session = new WorkoutSessionEntity
            {
                UserId = userId,
                WorkoutType = workoutType,
                StartTime = startUtc,
                EndTime = startUtc.AddMinutes(dto.DurationMins),
                DurationMins = dto.DurationMins,
                XPAwarded = Math.Max(10, dto.DurationMins * 2)
            };

            _db.WorkoutSessions.Add(session);
            AddDistance(session, dto.DistanceKm);
            return await RecordCompletedAsync(userId, session);
        }

        /// <summary>
        /// Records the distance as an exercise on the session, which is what Distance
        /// quests count. Ignored for workout types that don't cover distance.
        /// </summary>
        private void AddDistance(WorkoutSessionEntity session, float? distanceKm)
        {
            if (distanceKm is not > 0 || !WorkoutTypes.TracksDistance(session.WorkoutType)) return;

            var exercise = new ExerciseEntity
            {
                SessionId = session.SessionId,
                Name = "Distance",
                Category = session.WorkoutType.ToString(),
                DistanceKm = distanceKm
            };
            // Added via the DbSet: the preset key would otherwise make EF treat it as existing
            _db.Exercises.Add(exercise);
            if (!session.Exercises.Contains(exercise))
                session.Exercises.Add(exercise);
        }

        /// <summary>
        /// Rewards a finished workout: XP, streak, quest / challenge progress
        /// (and their XP) in one save, then achievements and stat points.
        /// </summary>
        private async Task<WorkoutDto> RecordCompletedAsync(Guid userId, WorkoutSessionEntity session)
        {
            var stats = await _goals.GetOrCreateStatsAsync(userId);
            var xpBefore = stats.TotalXP;
            var levelBefore = stats.CurrentLevel;
            var goldBefore = stats.Gold;

            _goals.GrantXP(stats, session.XPAwarded, XPReason.WorkoutCompleted);
            _goals.GrantGold(stats, GoldRules.ForWorkout(session.DurationMins), GoldReason.Workout,
                $"{session.WorkoutType} · {session.DurationMins} min");
            stats.TotalWorkouts++;

            var today = DateTime.UtcNow.Date;
            if (stats.LastWorkoutDate?.Date == today.AddDays(-1))
                stats.CurrentStreak++;
            else if (stats.LastWorkoutDate?.Date != today)
                stats.CurrentStreak = 1;
            stats.LongestStreak = Math.Max(stats.LongestStreak, stats.CurrentStreak);
            stats.LastWorkoutDate = DateTime.UtcNow;

            var completions = await _goals.AdvanceForWorkoutAsync(stats, session);

            await _db.SaveChangesAsync();

            var gain = StatRules.ForWorkout(session.WorkoutType, session.DurationMins);
            foreach (var completion in completions)
                gain += StatRules.ForCompletion(completion.Type, completion.Goal.XPReward);

            var dto = _mapper.Map<WorkoutDto>(session);
            dto.CharacterProgress = await _character.ApplyActivityAsync(userId, gain);

            // After evolving, so body-type achievements see the new build
            var achievements = await _achievements.CheckAndUnlockAsync(userId);
            dto.AccountProgress = new AccountProgressDto
            {
                XPGained = stats.TotalXP - xpBefore,
                GoldGained = stats.Gold - goldBefore,
                PreviousLevel = levelBefore,
                NewLevel = stats.CurrentLevel,
                CompletedGoals = completions.Select(c => c.Goal)
                    .Concat(achievements.Select(a => new CompletedGoalDto
                    {
                        Kind = "Achievement",
                        Id = a.AchievementId,
                        Title = a.Name,
                        XPReward = a.XPReward
                    }))
                    .ToList()
            };
            return dto;
        }

        public async Task<List<WorkoutDto>> GetMonthlyAsync(Guid userId, int year, int month)
        {
            var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to   = from.AddMonths(1);

            var sessions = await _db.WorkoutSessions
                .Include(s => s.Exercises)
                .Where(s => s.UserId == userId && s.EndTime != null
                         && s.StartTime >= from && s.StartTime < to)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return _mapper.Map<List<WorkoutDto>>(sessions);
        }

        public async Task<ExerciseDto> AddExerciseAsync(Guid userId, Guid sessionId, AddExerciseDto dto)
        {
            var session = await _db.WorkoutSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId)
                ?? throw new KeyNotFoundException("Workout session not found.");

            var exercise = new ExerciseEntity
            {
                SessionId = sessionId,
                Name = dto.Name,
                Category = dto.Category,
                Sets = dto.Sets,
                Reps = dto.Reps,
                WeightKg = dto.WeightKg,
                DistanceKm = dto.DistanceKm,
                DurationSecs = dto.DurationSecs
            };

            _db.Exercises.Add(exercise);
            await _db.SaveChangesAsync();
            return _mapper.Map<ExerciseDto>(exercise);
        }

        public async Task<List<WorkoutDto>> GetHistoryAsync(Guid userId, int page, int pageSize)
        {
            var sessions = await _db.WorkoutSessions
                .Include(s => s.Exercises)
                .Where(s => s.UserId == userId && s.EndTime != null)
                .OrderByDescending(s => s.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<WorkoutDto>>(sessions);
        }

        public async Task<WorkoutDto> GetSessionAsync(Guid userId, Guid sessionId)
        {
            var session = await _db.WorkoutSessions
                .Include(s => s.Exercises)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId)
                ?? throw new KeyNotFoundException("Workout session not found.");

            return _mapper.Map<WorkoutDto>(session);
        }
    }
}
