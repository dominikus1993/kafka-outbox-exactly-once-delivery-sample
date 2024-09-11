using Sample.Common.Messaging.Abstractions;

namespace Sample.Infrastructure.Messaging.Kafka.Configuration;

public sealed class KafkaConsumerConfig<T> where T: IMessage
{
    public string ClientId { get; init; } = Guid.NewGuid().ToString("D");
    
    public string Topic { get; init; }
}