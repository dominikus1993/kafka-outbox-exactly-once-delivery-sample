using Microsoft.EntityFrameworkCore;
using Sample.Core.Order.Model;
using Sample.Infrastructure.Orders.Model;

namespace Sample.Infrastructure.Orders.DbContexts;

public sealed class ProductsDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }
    
    public DbSet<OutBox> OutBox { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Order>(order =>
        {
            order.HasKey(x => new { x.Id, x.OrderNumber });
            order.HasIndex(x => x.OrderNumber);
            order.Property(x => x.State).HasConversion<string>();
            order.Property(x => x.OrderNumber).ValueGeneratedNever();
            order.Property(x => x.Id).ValueGeneratedNever();
            order.OwnsMany(x => x.Items, item =>
            {
                item.ToJson();
            });
        });
        modelBuilder.Entity<OutBox>(outBox =>
        {
            outBox.HasKey(x => x.Id);
            outBox.Property(x => x.Id).ValueGeneratedNever();
            outBox.HasIndex(x => x.ProcessedAtTimestamp);
            outBox.HasIndex(x => x.CreatedAtTimestamp);
            outBox.Property(x => x.Type).IsRequired().HasMaxLength(255);
            outBox.Property(x => x.Name).IsRequired().HasMaxLength(255);
            outBox.Property(x => x.Data).IsRequired().HasColumnType("jsonb");
            outBox.Property(x => x.CreatedAtTimestamp).IsRequired();
            outBox.Property(x => x.ProcessedAtTimestamp);
        });
    }

    public async Task CleanAllTables(CancellationToken cancellationToken = default)
    {
        await OutBox.ExecuteDeleteAsync(cancellationToken: cancellationToken);
        await Orders.ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }
}