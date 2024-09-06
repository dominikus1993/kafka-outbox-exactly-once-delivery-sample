using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Infrastructure.Orders.DbContexts;

namespace Sample.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<OrdersDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("Products"), contextOptionsBuilder =>
            {
            });
        });

        return services;
    }
}