using Microsoft.EntityFrameworkCore;
using Sample.Common.Types;
using Sample.Core.Events;
using Sample.Core.Order.Model;
using Sample.Core.Order.Repositories;
using Sample.Infrastructure.Orders.DbContexts;
using Sample.Infrastructure.Orders.Model;

namespace Sample.Infrastructure.Orders.Repositories;

public sealed class PostgresOrdersCreationRepository : IOrdersCreationRepository
{
    private readonly IDbContextFactory<OrdersDbContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;

    public PostgresOrdersCreationRepository(IDbContextFactory<OrdersDbContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }


    public async Task<Result<Guid>> CreateOrder(Order order, CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var tran = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            context.Orders.Add(order);
            var orderSaved = new OrderSaved(order, _timeProvider);
            context.OutBox.Add(new OutBox(orderSaved, _timeProvider));
            await context.SaveChangesAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            return Result.Ok(order.Id);
        }
        catch (Exception e)
        {
            await tran.RollbackAsync(cancellationToken);
            return Result.Failure<Guid>(e);
        }
    }
}