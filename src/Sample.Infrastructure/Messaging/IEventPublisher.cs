using Sample.Common.Types;
using Sample.Infrastructure.Orders.Model;

namespace Sample.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task<Result<Unit>> Publish(OutBox outBoxEvent, CancellationToken cancellationToken = default);
}