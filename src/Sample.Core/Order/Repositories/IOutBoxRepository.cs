using Sample.Core.Common.Types;

namespace Sample.Core.Order.Repositories;

public interface IOrderRepository
{
    Task<Result<OrderId>> CreateOrder(Model.Order order, CancellationToken cancellationToken);
}