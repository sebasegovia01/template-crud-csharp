using Google.Cloud.PubSub.V1;

namespace basetemplate_csharp.Services
{
    public class PubSubSubscriberService : BackgroundService
    {
        private readonly string _projectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? "";
        private readonly string _subscriptionId = Environment.GetEnvironmentVariable("GCP_SUB_ID") ?? "";
        private readonly ILogger<PubSubSubscriberService> _logger;
        private readonly RedisService _redisService;
        private readonly SubscriberClient _subscriber;

        public PubSubSubscriberService(ILogger<PubSubSubscriberService> logger, RedisService redisService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));

            var subscriptionName = SubscriptionName.FromProjectSubscription(_projectId, _subscriptionId);
            _subscriber = SubscriberClient.Create(subscriptionName);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _subscriber.StartAsync(async (PubsubMessage message, CancellationToken ct) =>
            {
                await ProcessMessageAsync(message, ct);
                return SubscriberClient.Reply.Ack;
            });
        }

        private async Task ProcessMessageAsync(PubsubMessage message, CancellationToken cancellationToken)
        {
            string messageId = message.MessageId;
            _logger.LogInformation($"Processing message with ID: {messageId}");

            string lockKey = $"lock:{messageId}";
            await using var lockScope = await _redisService.AcquireLockAsync(lockKey, TimeSpan.FromMinutes(5), cancellationToken);

            if (lockScope == null)
            {
                _logger.LogInformation($"Could not acquire lock for message ID: {messageId}. Skipping processing.");
                return;
            }

            try
            {
                if (await _redisService.CheckAndMarkAsProcessedAsync(messageId, cancellationToken))
                {
                    _logger.LogInformation($"Message {messageId} has already been processed. Skipping.");
                    return;
                }

                string messageData = message.Data.ToStringUtf8();
                _logger.LogInformation($"Message received: {messageData}");

                // Your message processing logic here
                await Task.Delay(100, cancellationToken);  // Simulating some work

                _logger.LogInformation($"Message {messageId} processing completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message {messageId}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _subscriber.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}