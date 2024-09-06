using Sample.Core.Common.Types;

namespace Sample.Core.Order.Repositories;

public interface IOrdersCreationRepository
{
    Task<Result<OrderId>> CreateOrder(Model.Order order, CancellationToken cancellationToken = default);
}