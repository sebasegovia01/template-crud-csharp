using StackExchange.Redis;

namespace basetemplate_csharp.Services
{
    public class RedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static readonly string CheckAndMarkScript = @"
            if redis.call('exists', KEYS[1]) == 1 then
                return 1
            else
                redis.call('set', KEYS[1], 'processed')
                redis.call('expire', KEYS[1], 600)  -- expire after 10 minutes
                return 0
            end";

        public async Task<bool> CheckAndMarkAsProcessedAsync(string messageId, CancellationToken cancellationToken)
        {
            var db = _redis.GetDatabase();
            var result = (int)await db.ScriptEvaluateAsync(CheckAndMarkScript, new RedisKey[] { messageId });
            bool wasAlreadyProcessed = result == 1;
            _logger.LogInformation($"Message {messageId} was already processed: {wasAlreadyProcessed}");
            return wasAlreadyProcessed;
        }

        public async Task<IAsyncDisposable> AcquireLockAsync(string lockKey, TimeSpan expiry, CancellationToken cancellationToken)
        {
            var db = _redis.GetDatabase();
            var lockValue = Guid.NewGuid().ToString();
            bool acquired = await db.LockTakeAsync(lockKey, lockValue, expiry);
            _logger.LogInformation($"Lock acquired for key {lockKey}: {acquired}");

            if (acquired)
            {
                return new AsyncLockScope(db, lockKey, lockValue, _logger);
            }

            throw new RedisLockAcquisitionException($"Failed to acquire lock for key: {lockKey}");
        }

        public async Task<bool> PingAsync(CancellationToken cancellationToken)
        {
            try
            {
                var db = _redis.GetDatabase();
                var pingResult = await db.ExecuteAsync("PING");
                _logger.LogInformation($"Redis PING result: {pingResult}");
                return pingResult.ToString() == "PONG";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis PING failed");
                return false;
            }
        }

        private class AsyncLockScope : IAsyncDisposable
        {
            private readonly IDatabase _db;
            private readonly string _lockKey;
            private readonly string _lockValue;
            private readonly ILogger _logger;

            public AsyncLockScope(IDatabase db, string lockKey, string lockValue, ILogger logger)
            {
                _db = db;
                _lockKey = lockKey;
                _lockValue = lockValue;
                _logger = logger;
            }

            public async ValueTask DisposeAsync()
            {
                await _db.LockReleaseAsync(_lockKey, _lockValue);
                _logger.LogInformation($"Lock released for key {_lockKey}");
            }
        }
    }

    public class RedisLockAcquisitionException : Exception
    {
        public RedisLockAcquisitionException(string message) : base(message) { }
    }
}