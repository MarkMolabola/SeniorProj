using MDRNJ.Shared.DTOs.Workout;
using MDRNJfitness.Data;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly HttpClient _http;
        private readonly WorkoutRepo _repo;

        public WorkoutService(HttpClient http, WorkoutRepo repo)
        {
            _http = http;
            _repo = repo;
        }

        public Task<WorkoutDto> StartAsync(CreateWorkoutDto dto) =>
            _http.PostAsync<WorkoutDto>("api/workouts/start", dto)!;

        public Task<WorkoutDto> AddExerciseAsync(Guid sessionId, AddExerciseDto dto) =>
            _http.PostAsync<WorkoutDto>($"api/workouts/{sessionId}/exercises", dto)!;

        public Task<WorkoutDto> CompleteAsync(Guid sessionId) =>
            _http.PutAsync<WorkoutDto>($"api/workouts/{sessionId}/end", new { })!;

        public Task<List<WorkoutDto>> GetHistoryAsync(int page = 1, int count = 10) =>
            _http.GetAsync<List<WorkoutDto>>($"api/workouts/history?page={page}&pageSize={count}")!;

        public Task<List<WorkoutDto>> GetMonthlyAsync(int year, int month) =>
            _http.GetAsync<List<WorkoutDto>>($"api/workouts/month?year={year}&month={month}")!;
    }
}
