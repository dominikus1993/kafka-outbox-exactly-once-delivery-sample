using Microsoft.EntityFrameworkCore;
using Sample.Core.Common.Types;
using Sample.Core.Order.Model;
using Sample.Core.Order.Repositories;
using Sample.Infrastructure.Orders.DbContexts;

namespace Sample.Infrastructure.Orders.Repositories;

public class PostgresOrdersReader : IOrdersReader
{
    private readonly IDbContextFactory<OrdersDbContext> _dbContextFactory;
    
    public Task<Result<Order>> FindById(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}