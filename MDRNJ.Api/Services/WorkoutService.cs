using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Workout;
using MDRNJ.Shared.Enums;
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

        public WorkoutService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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

        public async Task<WorkoutDto> EndWorkoutAsync(Guid userId, Guid sessionId)
        {
            var session = await _db.WorkoutSessions
                .Include(s => s.Exercises)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId)
                ?? throw new KeyNotFoundException("Workout session not found.");

            if (session.EndTime.HasValue)
                throw new InvalidOperationException("Workout already ended.");

            session.EndTime = DateTime.UtcNow;
            session.DurationMins = (int)(session.EndTime.Value - session.StartTime).TotalMinutes;

            int xp = Math.Max(10, session.DurationMins * 2);
            session.XPAwarded = xp;

            // Update game stats in the same unit of work so one SaveChangesAsync
            // covers the workout + stats + XP transaction atomically.
            var stats = await _db.GameStats.FirstOrDefaultAsync(g => g.UserId == userId);
            if (stats is null)
            {
                stats = new GameStatsEntity { UserId = userId };
                _db.GameStats.Add(stats);
            }

            stats.TotalXP += xp;
            stats.CurrentLevel = LevelFromXP(stats.TotalXP);
            stats.TotalWorkouts++;

            var today = DateTime.UtcNow.Date;
            if (stats.LastWorkoutDate?.Date == today.AddDays(-1))
            {
                stats.CurrentStreak++;
                if (stats.CurrentStreak > stats.LongestStreak)
                    stats.LongestStreak = stats.CurrentStreak;
            }
            else if (stats.LastWorkoutDate?.Date != today)
            {
                stats.CurrentStreak = 1;
            }
            stats.LastWorkoutDate = DateTime.UtcNow;

            _db.XPTransactions.Add(new XPTransactionEntity
            {
                UserId = userId,
                Amount = xp,
                Reason = XPReason.WorkoutCompleted
            });

            await _db.SaveChangesAsync();

            return _mapper.Map<WorkoutDto>(session);
        }

        private static int LevelFromXP(int xp)
        {
            int level = 1;
            while (xp >= level * level * 100) level++;
            return level;
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
