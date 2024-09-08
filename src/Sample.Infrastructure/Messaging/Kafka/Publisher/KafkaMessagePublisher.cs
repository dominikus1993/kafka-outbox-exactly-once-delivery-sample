using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Sample.Common.Messaging.Abstractions;
using Sample.Common.Types;
using Sample.Infrastructure.Messaging.Kafka.Serialization;

namespace Sample.Infrastructure.Messaging.Kafka.Publisher;

public sealed class KafkaMessagePublisher : IMessagePublisher
{
    private readonly IServiceProvider _serviceProvider;

    public KafkaMessagePublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<Result<Unit>> Publish<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(message);
        
            await using var sp = _serviceProvider.CreateAsyncScope();
            var config = sp.ServiceProvider.GetRequiredService<KafkaMessagePublisherConfig<TMessage>>();
            var headers = new Headers
            {
                { "message_type", JsonSerializer.SerializeToUtf8Bytes(typeof(TMessage).FullName) },
                { "message_id", JsonSerializer.SerializeToUtf8Bytes(message.MessageId) }
            };
            var serializer = sp.ServiceProvider.GetRequiredService<IKafkaMessageSerializer<TMessage>>();
            var json = JsonSerializer.SerializeToUtf8Bytes(message, MessagesSerializationConfig.Default.Options);
        
            var key = config.KeySelector(message);
            var producer = sp.ServiceProvider.GetRequiredService<KafkaDependentProducer<string, byte[]>>();
            await producer.ProduceAsync(config.Topic, new Message<string, byte[]> { Key = key, Value = json, Headers = headers }, cancellationToken);
            
            return Result.UnitResult;
        }
        catch (Exception e)
        {
            return Result.Failure<Unit>(e);
        }
    }
}