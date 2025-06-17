using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PaymentService.Data;
using PaymentService.Dtos;
using PaymentService.Dtos.Payment;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class InboxProcessor : BackgroundService
    {
        private readonly ILogger<InboxProcessor> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaSettings _kafkaSettings;

        public InboxProcessor(IServiceScopeFactory scopeFactory, ILogger<InboxProcessor> logger, IOptions<KafkaSettings> kafkaOptions)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _kafkaSettings = kafkaOptions.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var thread = new Thread(() => StartConsumerLoop(stoppingToken));
            thread.Start();
            return Task.CompletedTask;
        }

        private void StartConsumerLoop(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = _kafkaSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false // ручной commit
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(_kafkaSettings.Topics.OrderEvents);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(cancellationToken);

                    var key = cr.Message.Key;
                    var value = cr.Message.Value;


                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    if (db.InboxMessages.Any(x => x.MessageId == cr.Message.Key && x.Topic == cr.Topic))
                    {
                        _logger.LogInformation("Пропущено дублирующее сообщение {Key}", key);
                        consumer.Commit(cr);
                        continue;
                    }

                    var orderDto = JsonSerializer.Deserialize<PaymentRequest>(value);
                    if (orderDto is null)
                    {
                        _logger.LogError("Ошибка десериализации сообщения: {Value}", value);
                        continue;
                    }
                    ProcessPayment(orderDto, db);

                    db.InboxMessages.Add(new InboxMessage
                    {
                        MessageId = cr.Message.Key,
                        Payload = value,
                        Topic = cr.Topic,
                        ProcessedAt = DateTime.UtcNow
                    });

                    db.SaveChanges();
                    consumer.Commit(cr);
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "Kafka consume error");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения Kafka");
                }
            }

            consumer.Close();
        }

        private void ProcessPayment(PaymentRequest order, AppDbContext db)
        {
            var account = db.Accounts.FirstOrDefault(a => a.UserId == order.UserId);
            string eventType;
            object eventPayload;


            var paymentId = Guid.NewGuid();
            bool isSuccess = account != null && order.Amount > 0 && account.Balance >= order.Amount;

            if (isSuccess)
            {
                account.Balance -= order.Amount;
            }

            eventType = isSuccess ? "PaymentCompleted" : "PaymentFailed";
            eventPayload = new
            {
                PaymentId = paymentId,
                OrderId = order.OrderId,
                Status = (int)(isSuccess ? OrderStatus.FINISHED : OrderStatus.CANCELLED)
            };


            var outbox = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = eventType,
                OccurredAt = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(eventPayload)
            };

            db.OutboxMessages.Add(outbox);

            db.SaveChanges();
        }
    }

}
