using Sample.Common.Messaging.Abstractions;

namespace Sample.Infrastructure.Messaging.Kafka.Publisher;

public sealed class KafkaMessagePublisherConfig<T> where T : IMessage
{
    private static readonly Func<T, string> DefaultKeySelector = message => message.MessageId.ToString();
    
    public KafkaMessagePublisherConfig(string topic, Func<T, string>? keySelector = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(topic);
        KeySelector = keySelector ?? DefaultKeySelector;
        Topic = topic;
    }

    public string Topic { get; }
        
    public Func<T, string> KeySelector { get; }
}