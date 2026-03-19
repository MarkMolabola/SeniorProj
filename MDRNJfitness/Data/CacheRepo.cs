using Newtonsoft.Json;

namespace MDRNJfitness.Data
{
    public class CacheRepo
    {
        private readonly LocalDatabase _db;
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(1);

        public CacheRepo(LocalDatabase db) => _db = db;

        public async Task SetAsync<T>(string key, T value)
        {
            var conn = await _db.GetConnectionAsync();
            var record = new CachedUserRecord
            {
                Key = key,
                Json = JsonConvert.SerializeObject(value),
                CachedAt = DateTime.UtcNow
            };
            await conn.InsertOrReplaceAsync(record);
        }

        public async Task<T?> GetAsync<T>(string key, TimeSpan? ttl = null)
        {
            var conn = await _db.GetConnectionAsync();
            var record = await conn.FindAsync<CachedUserRecord>(key);
            if (record is null) return default;

            var age = DateTime.UtcNow - record.CachedAt;
            if (age > (ttl ?? DefaultTtl))
            {
                await conn.DeleteAsync(record);
                return default;
            }

            return JsonConvert.DeserializeObject<T>(record.Json);
        }

        public async Task RemoveAsync(string key)
        {
            var conn = await _db.GetConnectionAsync();
            await conn.DeleteAsync<CachedUserRecord>(key);
        }

        public async Task ClearAsync()
        {
            var conn = await _db.GetConnectionAsync();
            await conn.DeleteAllAsync<CachedUserRecord>();
        }
    }
}
