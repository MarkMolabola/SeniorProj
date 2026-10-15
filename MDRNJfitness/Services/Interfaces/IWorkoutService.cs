using MDRNJ.Shared.DTOs.Workout;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IWorkoutService
    {
        Task<WorkoutDto> StartAsync(CreateWorkoutDto dto);
        Task<WorkoutDto> AddExerciseAsync(Guid sessionId, AddExerciseDto dto);
        Task<WorkoutDto> CompleteAsync(Guid sessionId, float? distanceKm = null);
        Task<WorkoutDto> BacklogAsync(BacklogWorkoutDto dto);
        Task<List<WorkoutDto>> GetHistoryAsync(int page = 1, int count = 10);
        Task<List<WorkoutDto>> GetMonthlyAsync(int year, int month);
    }
}
