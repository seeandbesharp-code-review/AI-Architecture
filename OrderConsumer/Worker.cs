using Confluent.Kafka;

namespace OrderConsumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string _bootstrapServers;
    private readonly string _topic;
    private readonly string _groupId = "webapishop-consumer-group";

    public Worker(IConfiguration configuration, ILogger<Worker> logger)
    {
        _logger = logger;
        _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topics:Orders"] ?? "shop-orders";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            try
            {
                Consume(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Kafka consumer failed to start or lost connection: {Message}", ex.Message);
            }
        }, stoppingToken);
    }

    private void Consume(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_topic);

        _logger.LogInformation("Kafka consumer started. Listening on topic '{Topic}'...", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                _logger.LogInformation(
                    "Order event received | Topic: {Topic} | Partition: {Partition} | Offset: {Offset} | Key: {Key} | Value: {Value}",
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value,
                    result.Message.Key,
                    result.Message.Value);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer stopping.");
        }
        finally
        {
            consumer.Close();
        }
    }
}
