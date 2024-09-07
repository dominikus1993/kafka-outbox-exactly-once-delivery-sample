using System.Text.Json;
using System.Text.Json.Serialization;
using Sample.Common.Messaging.Abstractions;
using Sample.Common.Types;
using Sample.Core.Events;
using Sample.Core.Order.Model;
using Sample.Infrastructure.Orders.Model;

namespace Sample.Infrastructure.Messaging;

public sealed class DefaultEventPublisher : IEventPublisher
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly TimeProvider _timeProvider;
    public DefaultEventPublisher(IMessagePublisher messagePublisher, TimeProvider timeProvider)
    {
        _messagePublisher = messagePublisher;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Unit>> Publish(OutBox outBoxEvent, CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Deserialize<OutBoxEvent>(outBoxEvent.Data);
        var timestamp = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        switch (data)
        {
            case OrderSaved msg:
                var message = new OrderSavedMessage
                {
                    OrderId = msg.OrderId,
                    Items = msg.Items,
                    MessageId = outBoxEvent.Id,
                    SendAt = timestamp
                };
                return await _messagePublisher.Publish(message, cancellationToken);
        }

        return Result.UnitResult;
    }
    
}

internal sealed class OrderSavedMessage : IMessage
{
    public Guid OrderId { get; init; }
    public IReadOnlyList<OrderItem> Items { get; init; } = null!;
    public Guid MessageId { get; init; }
    public long SendAt { get; init; }
}

[JsonSerializable(typeof(OrderSavedMessage))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class MessagesSerializationConfig : JsonSerializerContext;