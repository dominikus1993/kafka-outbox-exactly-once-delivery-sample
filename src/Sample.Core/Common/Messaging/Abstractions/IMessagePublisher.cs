using Sample.Core.Common.Types;

namespace Sample.Core.Common.Messaging.Abstractions;

public interface IMessagePublisher
{
    ValueTask<Result<Unit>> Publish<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : IMessage;
}