using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Workout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/workouts")]
    [Authorize]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutService _workouts;

        public WorkoutController(IWorkoutService workouts) => _workouts = workouts;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] CreateWorkoutDto dto) =>
            Ok(await _workouts.StartWorkoutAsync(UserId, dto));

        [HttpPut("{sessionId:guid}/end")]
        public async Task<IActionResult> End(Guid sessionId) =>
            Ok(await _workouts.EndWorkoutAsync(UserId, sessionId));

        [HttpPost("{sessionId:guid}/exercises")]
        public async Task<IActionResult> AddExercise(Guid sessionId, [FromBody] AddExerciseDto dto) =>
            Ok(await _workouts.AddExerciseAsync(UserId, sessionId, dto));

        [HttpGet("history")]
        public async Task<IActionResult> History([FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
            Ok(await _workouts.GetHistoryAsync(UserId, page, pageSize));

        [HttpGet("{sessionId:guid}")]
        public async Task<IActionResult> GetSession(Guid sessionId) =>
            Ok(await _workouts.GetSessionAsync(UserId, sessionId));

        [HttpGet("month")]
        public async Task<IActionResult> Monthly([FromQuery] int year, [FromQuery] int month) =>
            Ok(await _workouts.GetMonthlyAsync(UserId, year, month));
    }
}
