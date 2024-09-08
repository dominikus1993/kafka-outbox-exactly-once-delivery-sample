using Confluent.Kafka;

namespace Sample.Infrastructure.Messaging.Kafka.Configuration;

public class KafkaConfiguration
{
    public bool Enabled { get; set; }
    public ProducerConfig? ProducerConfig { get; set; }
    public ConsumerConfig? ConsumerConfig { get; set; }

    public override string ToString()
    {
        return $"Enabled: {Enabled}, ProducerConfig: {ProducerConfig}, ConsumerConfig: {ConsumerConfig}";
    }
}