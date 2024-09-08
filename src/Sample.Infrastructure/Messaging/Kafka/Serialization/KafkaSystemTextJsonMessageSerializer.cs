using System.Text.Json;
using Confluent.Kafka;
using Sample.Common.Messaging.Abstractions;

namespace Sample.Infrastructure.Messaging.Kafka.Serialization;

public interface IKafkaMessageSerializer<T> : IDeserializer<T>, ISerializer<T> where T : class, IMessage;

internal static class  KafkaSystemTextJsonMessageSerializer 
{
    public static readonly JsonSerializerOptions DefaultOptions = JsonSerializerOptions.Default;
}


public sealed class KafkaSystemTextJsonMessageSerializer<T> : IKafkaMessageSerializer<T> where T : class, IMessage
{
    private readonly JsonSerializerOptions _serializerOptions;
    public KafkaSystemTextJsonMessageSerializer(JsonSerializerOptions? options = null)
    {
        _serializerOptions = options ?? KafkaSystemTextJsonMessageSerializer.DefaultOptions;
    }
    
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            return null!;
        }

        return JsonSerializer.Deserialize<T>(data, _serializerOptions)!;
    }

    public byte[] Serialize(T data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data, _serializerOptions);
    }
}