using Sample.Core.Common.Types;

namespace Sample.Core.Order.Repositories;

public interface IOrdersReader
{
    Task<Result<Model.Order?>> FindById(OrderId id, CancellationToken cancellationToken = default);
}