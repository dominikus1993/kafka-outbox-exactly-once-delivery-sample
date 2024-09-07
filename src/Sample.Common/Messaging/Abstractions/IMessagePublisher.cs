using Sample.Common.Types;

namespace Sample.Common.Messaging.Abstractions;

public interface IMessagePublisher
{
    ValueTask<Result<Unit>> Publish<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : IMessage;
}