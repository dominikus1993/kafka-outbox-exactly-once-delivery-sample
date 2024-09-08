using Sample.Common.Messaging.Abstractions;
using Sample.Common.Types;

namespace Sample.Infrastructure.Messaging.Kafka.Publisher;

public sealed class NoopMessagePublisher : IMessagePublisher
{
    public ValueTask<Result<Unit>> Publish<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        return ValueTask.FromResult<Result<Unit>>(Result.UnitResult);
    }
}