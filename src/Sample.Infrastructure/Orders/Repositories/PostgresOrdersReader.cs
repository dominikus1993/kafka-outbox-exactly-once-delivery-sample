using Microsoft.EntityFrameworkCore;
using Sample.Common.Types;
using Sample.Core.Order.Model;
using Sample.Core.Order.Repositories;
using Sample.Infrastructure.Orders.DbContexts;

namespace Sample.Infrastructure.Orders.Repositories;

public sealed class OrderNotFoundException(Guid id) : Exception($"Order with id {id} was not found.")
{
    public Guid OrderId { get; } = id;

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(OrderId)}: {OrderId}";
    }
}

public sealed class PostgresOrdersReader : IOrdersReader
{
    private readonly IDbContextFactory<OrdersDbContext> _dbContextFactory;

    public PostgresOrdersReader(IDbContextFactory<OrdersDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<Order?>> FindById(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var res = await context.ReadOnlyOrderById(id, cancellationToken);
        if (res is not null)
        {
            return Result.Ok<Order?>(res);
        }
        return Result.Failure<Order?>(new OrderNotFoundException(id));
    }
}