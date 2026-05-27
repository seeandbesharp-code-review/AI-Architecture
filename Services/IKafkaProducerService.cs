namespace Services;

public interface IKafkaProducerService
{
    Task ProduceAsync(string topic, string key, string value);
}
