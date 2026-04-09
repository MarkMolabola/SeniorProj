using MDRNJfitness.Data;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class SyncService : ISyncService
    {
        private readonly WorkoutRepo _workouts;

        public SyncService(WorkoutRepo workouts) => _workouts = workouts;

        public async Task SyncPendingWorkoutsAsync()
        {
            var pending = await _workouts.GetPendingAsync();
            // TODO: upload each pending workout to the API, then mark as synced
            _ = pending;
        }
    }
}
