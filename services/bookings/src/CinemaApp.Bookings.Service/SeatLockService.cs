using StackExchange.Redis;

namespace CinemaApp.Bookings.Service;

// This is THE feature of the project: a short-lived, distributed lock on a
// seat backed by Redis. SET key value NX EX <ttl> is atomic, so two users
// clicking the same seat at the same millisecond cannot both "win" - exactly
// the double-booking problem this design solves.
public interface ISeatLockService
{
    Task<bool> TryLockSeatAsync(string screeningId, string seatId, string userId, TimeSpan ttl);
    Task<string?> GetLockOwnerAsync(string screeningId, string seatId);
    Task<bool> ReleaseLockAsync(string screeningId, string seatId, string userId);
    Task ReleaseLocksAsync(string screeningId, IEnumerable<string> seatIds);
}

public class RedisSeatLockService : ISeatLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisSeatLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    private static string Key(string screeningId, string seatId) => $"seatlock:{screeningId}:{seatId}";

    public async Task<bool> TryLockSeatAsync(string screeningId, string seatId, string userId, TimeSpan ttl)
    {
        var db = _redis.GetDatabase();
        // NX = only set if it does not already exist -> atomic "first one wins".
        return await db.StringSetAsync(Key(screeningId, seatId), userId, ttl, When.NotExists);
    }

    public async Task<string?> GetLockOwnerAsync(string screeningId, string seatId)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(Key(screeningId, seatId));
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> ReleaseLockAsync(string screeningId, string seatId, string userId)
    {
        var db = _redis.GetDatabase();
        var key = Key(screeningId, seatId);
        var current = await db.StringGetAsync(key);

        // Only the user who holds the lock can release it.
        if (current.HasValue && current.ToString() == userId)
        {
            return await db.KeyDeleteAsync(key);
        }

        return false;
    }

    public async Task ReleaseLocksAsync(string screeningId, IEnumerable<string> seatIds)
    {
        var db = _redis.GetDatabase();
        var keys = seatIds.Select(seatId => (RedisKey)Key(screeningId, seatId)).ToArray();
        if (keys.Length > 0)
        {
            await db.KeyDeleteAsync(keys);
        }
    }
}
