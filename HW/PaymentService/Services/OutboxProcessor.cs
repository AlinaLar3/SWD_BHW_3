using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentService.Data;

namespace PaymentService.Services
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaSettings _kafkaSettings;

        public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger, IOptions<KafkaSettings> kafkaOptions)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _kafkaSettings = kafkaOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers
            };

            using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var messages = await db.OutboxMessages
                        .Where(m => m.ProcessedAt == null)
                        .OrderBy(m => m.OccurredAt)
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    foreach (var msg in messages)
                    {
                        var kafkaKey = msg.Id.ToString();
                        var kafkaValue = msg.Payload;


                        await producer.ProduceAsync(_kafkaSettings.Topics.PaymentEvents, new Message<string, string>
                        {
                            Key = kafkaKey,
                            Value = kafkaValue
                        }, stoppingToken);

                        msg.ProcessedAt = DateTime.UtcNow;
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (ProduceException<string, string> ex)
                {
                    _logger.LogError(ex, "Kafka produce error");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in OutboxProcessor");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}