using AutoFixture.Xunit2;
using Microsoft.Extensions.Time.Testing;
using Sample.Core.Order.Model;
using Sample.Core.Order.Repositories;
using Sample.Infrastructure.Orders.DbContexts;
using Sample.Infrastructure.Orders.Repositories;
using Sample.Infrastructure.Tests.Fixtures;
using Xunit;

namespace Sample.Infrastructure.Tests.Orders.Repositories;

public sealed class SqlOrderRepositoryTests : IClassFixture<PostgresOrdersFixture>, IAsyncLifetime
{
    private readonly PostgresOrdersFixture _postgresFixture;
    private readonly IOrdersCreationRepository _ordersCreationRepository;
    private readonly IOrdersReader _ordersReader;
    private readonly OrdersDbContext _productsDbContext;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    public SqlOrderRepositoryTests(PostgresOrdersFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _productsDbContext = _postgresFixture.DbContextFactory.CreateDbContext();
        _ordersCreationRepository = new PostgresOrdersCreationRepository(_postgresFixture.DbContextFactory, new FakeTimeProvider(_now));
        _ordersReader = new PostgresOrdersReader(_postgresFixture.DbContextFactory);
    }

    [Theory]
    [AutoData]
    public async Task TestSaveNewOrder(Guid orderId, long orderNumber, OrderItem[] items)
    {
        var order = Order.New(orderId, orderNumber, items);
        var res = await _ordersCreationRepository.CreateOrder(order);
        
        Assert.True(res.IsSuccess);

        var orderFromDbResult = await _ordersReader.FindById(res.Value);
        
        Assert.True(orderFromDbResult.IsSuccess);
        var orderFromDb = orderFromDbResult.Value;
        Assert.NotNull(orderFromDb);
        Assert.Equal(OrderState.New, orderFromDb.State);
    }
    
    [Theory]
    [AutoData]
    public async Task TestFindByIdWhenNoExists(Guid orderId)
    {
        var orderFromDbResult = await _ordersReader.FindById(orderId);
        Assert.False(orderFromDbResult.IsSuccess);
        var ex = orderFromDbResult.ErrorValue;
        Assert.IsType<OrderNotFoundException>(ex);
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _productsDbContext.CleanAllTables();
        await _productsDbContext.DisposeAsync();
    }
}