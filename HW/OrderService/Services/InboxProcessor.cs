using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.Data;
using OrderService.Dtos.Payment;
using OrderService.Models;

namespace OrderService.Services
{
    public class InboxProcessor : BackgroundService
    {
        private readonly ILogger<InboxProcessor> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaSettings _kafkaSettings;

        public InboxProcessor(IServiceScopeFactory scopeFactory, ILogger<InboxProcessor> logger, IOptions<KafkaSettings> kafkaSettings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _kafkaSettings = kafkaSettings.Value;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoopAsync(stoppingToken), stoppingToken);

        }

        private async Task StartConsumerLoopAsync(CancellationToken stoppingToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = _kafkaSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(_kafkaSettings.Topics.PaymentEvents);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);

                    _logger.LogInformation("Consumed message at {TopicPartitionOffset}: {Value}", cr.TopicPartitionOffset, cr.Message.Value);

                    var paymentEvent = System.Text.Json.JsonSerializer.Deserialize<PaymentCompleted>(cr.Message.Value);
                    if (paymentEvent == null)
                    {
                        _logger.LogWarning("Failed to deserialize payment event");
                        consumer.Commit(cr);
                        continue;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == paymentEvent.OrderId, stoppingToken);
                    if (order != null)
                    {
                        order.Status = paymentEvent.Status == (int)OrderStatus.FINISHED ? OrderStatus.FINISHED : OrderStatus.CANCELLED;
                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Order {OrderId} updated with status {Status}", paymentEvent.OrderId, order.Status);
                    }
                    else
                    {
                        _logger.LogWarning("Order not found: {OrderId}", paymentEvent.OrderId);
                    }

                    consumer.Commit(cr);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("InboxProcessor cancellation requested.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in InboxProcessor");
                }
            }

            consumer.Close();
        }

    }
}
