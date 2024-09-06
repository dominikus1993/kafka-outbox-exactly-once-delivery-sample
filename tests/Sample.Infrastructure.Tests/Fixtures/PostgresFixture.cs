using Microsoft.EntityFrameworkCore;
using Sample.Infrastructure.Orders.DbContexts;
using Testcontainers.PostgreSql;
using Xunit;

namespace Sample.Infrastructure.Tests.Fixtures;

internal sealed class ProductsDbContextFactory : IDbContextFactory<OrdersDbContext>
{
    private readonly DbContextOptions<OrdersDbContext> _options;

    public ProductsDbContextFactory(DbContextOptionsBuilder<OrdersDbContext> optionsBuilder)
    {
        _options = optionsBuilder.Options;
    }
    public OrdersDbContext CreateDbContext()
    {
        return new OrdersDbContext(_options);
    }
}

public sealed class PostgresOrdersFixture : IAsyncLifetime
{
    public readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder().Build();
    public IDbContextFactory<OrdersDbContext> DbContextFactory { get; private set; }
    
    public PostgresOrdersFixture()
    {
        
    }
    
    public async Task InitializeAsync()
    {
        await PostgreSqlContainer.StartAsync();
        var builder = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(this.PostgreSqlContainer.GetConnectionString(),
                optionsBuilder =>
                {
                    optionsBuilder.MigrationsAssembly(typeof(OrdersDbContext).Assembly.FullName);
                    optionsBuilder.CommandTimeout(500);
                });
        DbContextFactory = new ProductsDbContextFactory(builder);
        await MigrateAsync();
    }

    private async Task MigrateAsync()
    {
        await using var context = await DbContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }
    
    public async Task Clean()
    {
        await using var context = await DbContextFactory.CreateDbContextAsync();
        await context.CleanAllTables();
    }

    public async Task DisposeAsync()
    {
        await PostgreSqlContainer.DisposeAsync();
    }
}