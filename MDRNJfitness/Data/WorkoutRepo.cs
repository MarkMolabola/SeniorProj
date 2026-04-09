using MDRNJ.Shared.Enums;
using Newtonsoft.Json;

namespace MDRNJfitness.Data
{
    public class WorkoutRepo
    {
        private readonly LocalDatabase _db;

        public WorkoutRepo(LocalDatabase db) => _db = db;

        public async Task SavePendingAsync(object workoutDto)
        {
            var conn = await _db.GetConnectionAsync();
            await conn.InsertAsync(new PendingWorkoutRecord
            {
                WorkoutJson = JsonConvert.SerializeObject(workoutDto),
                SyncStatusValue = (int)SyncStatus.Pending,
                RecordedAt = DateTime.UtcNow
            });
        }

        public async Task<List<PendingWorkoutRecord>> GetPendingAsync()
        {
            var conn = await _db.GetConnectionAsync();
            return await conn.Table<PendingWorkoutRecord>()
                .Where(r => r.SyncStatusValue == (int)SyncStatus.Pending)
                .ToListAsync();
        }

        public async Task MarkSyncedAsync(Guid localId)
        {
            var conn = await _db.GetConnectionAsync();
            var record = await conn.FindAsync<PendingWorkoutRecord>(localId);
            if (record is null) return;
            record.SyncStatusValue = (int)SyncStatus.Synced;
            await conn.UpdateAsync(record);
        }

        public async Task DeleteAsync(Guid localId)
        {
            var conn = await _db.GetConnectionAsync();
            await conn.DeleteAsync<PendingWorkoutRecord>(localId);
        }
    }
}
