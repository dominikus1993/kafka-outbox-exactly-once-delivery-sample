using Sample.Common.Types;

namespace Sample.Common.Messaging.Abstractions;

public interface IMessageContext { }

public interface IMessageConsumer<in T> where T: IMessage
{
    Task<Result<Unit>> Consume(T message, IMessageContext context, CancellationToken cancellationToken);
}