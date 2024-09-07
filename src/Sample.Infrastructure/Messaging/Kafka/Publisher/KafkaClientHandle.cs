using Confluent.Kafka;

namespace Sample.Infrastructure.Messaging.Kafka.Publisher;

internal sealed class KafkaClientHandle : IDisposable
{
    private readonly IProducer<byte[], byte[]> _kafkaProducer;
    public KafkaClientHandle(ProducerConfig? config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _kafkaProducer = new ProducerBuilder<byte[], byte[]>(config).Build();
    }

    public Handle Handle => _kafkaProducer.Handle;

    public void Dispose()
    {
        // Block until all outstanding produce requests have completed (with or
        // without error).
        _kafkaProducer.Flush();
        _kafkaProducer.Dispose();
    }
}