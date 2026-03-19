using MDRNJ.Shared.DTOs.Workout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IWorkoutService
    {
        Task<WorkoutDto> StartWorkoutAsync(Guid userId, CreateWorkoutDto dto);
        Task<WorkoutDto> EndWorkoutAsync(Guid userId, Guid sessionId);
        Task<ExerciseDto> AddExerciseAsync(Guid userId, Guid sessionId, AddExerciseDto dto);
        Task<List<WorkoutDto>> GetHistoryAsync(Guid userId, int page, int pageSize);
        Task<List<WorkoutDto>> GetMonthlyAsync(Guid userId, int year, int month);
        Task<WorkoutDto> GetSessionAsync(Guid userId, Guid sessionId);
    }
}
