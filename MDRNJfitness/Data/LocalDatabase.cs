using SQLite;

namespace MDRNJfitness.Data
{
    public class LocalDatabase
    {
        private SQLiteAsyncConnection? _db;

        private async Task InitAsync()
        {
            if (_db is not null) return;
            var path = Path.Combine(FileSystem.AppDataDirectory, "mdrnj.db3");
            _db = new SQLiteAsyncConnection(path);
            await _db.CreateTableAsync<PendingWorkoutRecord>();
            await _db.CreateTableAsync<CachedUserRecord>();
        }

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            await InitAsync();
            return _db!;
        }
    }

    [Table("PendingWorkouts")]
    public class PendingWorkoutRecord
    {
        [PrimaryKey]
        public Guid LocalId { get; set; } = Guid.NewGuid();
        public string WorkoutJson { get; set; } = null!;
        public int SyncStatusValue { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    [Table("CachedUsers")]
    public class CachedUserRecord
    {
        [PrimaryKey]
        public string Key { get; set; } = null!;
        public string Json { get; set; } = null!;
        public DateTime CachedAt { get; set; }
    }
}
