using AutoFixture.Xunit2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Sample.Common.Types;
using Sample.Core.Order.Model;
using Sample.Core.Order.Repositories;
using Sample.Infrastructure.Messaging;
using Sample.Infrastructure.Orders.DbContexts;
using Sample.Infrastructure.Orders.Model;
using Sample.Infrastructure.Orders.Repositories;
using Sample.Infrastructure.Orders.Services;
using Sample.Infrastructure.Tests.Fixtures;
using Xunit;

namespace Sample.Infrastructure.Tests.Orders.Services;

file sealed class FakeEventPublisher : IEventPublisher
{
    private List<OutBox> _outBoxes = [];
    public IReadOnlyList<OutBox> OutBoxes => _outBoxes;
    private readonly bool _isFailing;
    
    public FakeEventPublisher(bool isFailing = false)
    {
        _isFailing = isFailing;
    }
    
    public Task<Result<Unit>> Publish(OutBox outBoxEvent, CancellationToken cancellationToken = default)
    {
        if (_isFailing)
        {
            return Task.FromResult(Result.Failure<Unit>(new InvalidOperationException()));
        }
        
        _outBoxes.Add(outBoxEvent);
        return Task.FromResult(Result.UnitResult);
    }
}

public class PostgresOutBoxProcessorTests: IClassFixture<PostgresOrdersFixture>, IAsyncLifetime
{
    private readonly PostgresOrdersFixture _postgresFixture;
    private readonly IOrdersCreationRepository _ordersCreationRepository;
    private readonly IOrdersReader _ordersReader;
    private readonly OrdersDbContext _productsDbContext;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    
    public PostgresOutBoxProcessorTests(PostgresOrdersFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _productsDbContext = _postgresFixture.DbContextFactory.CreateDbContext();
        _ordersCreationRepository = new PostgresOrdersCreationRepository(_postgresFixture.DbContextFactory, new FakeTimeProvider(_now));
        _ordersReader = new PostgresOrdersReader(_postgresFixture.DbContextFactory);
    }
    
    [Theory]
    [AutoData]
    public async Task TestSendUnprocessedEventsWhenExists(Guid orderId, long orderNumber, Guid orderId2, long orderNumber2, OrderItem[] items)
    {
        
        // Arrange
        var order = Order.New(orderId, orderNumber, items);
        var res = await _ordersCreationRepository.CreateOrder(order);
        
        Assert.True(res.IsSuccess);
        
        var order2 = Order.New(orderId2, orderNumber2, items);
        var res2 = await _ordersCreationRepository.CreateOrder(order2);

        Assert.True(res2.IsSuccess);

        var publisher = new FakeEventPublisher();
        
        var outBoxService = new PostgresOutBoxProcessor(_postgresFixture.DbContextFactory, publisher, new FakeTimeProvider(_now));
        
        // Act
        
        var result = await outBoxService.ProcessEvents();
        
        var unprocessedEvents = await _productsDbContext.OutBox.AsNoTracking().Where(x => x.ProcessedAtTimestamp == null).ToListAsync();
        
        // Assert
        
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(publisher.OutBoxes);
        Assert.Empty(unprocessedEvents);
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