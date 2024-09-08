using Confluent.Kafka;

namespace Sample.Infrastructure.Messaging.Kafka.Publisher;

internal sealed class KafkaDependentProducer<K, V>
{
    private readonly IProducer<K, V> _kafkaHandle;

    public KafkaDependentProducer(KafkaClientHandle handle)
    {
        _kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle).Build();
    }
    
    public Task ProduceAsync(string topic, Message<K, V> message, CancellationToken cancellationToken = default)
        => _kafkaHandle.ProduceAsync(topic, message, cancellationToken);
    
    public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>>? deliveryHandler = null)
        => _kafkaHandle.Produce(topic, message, deliveryHandler);

    public void Flush(TimeSpan timeout)
        => _kafkaHandle.Flush(timeout);
}