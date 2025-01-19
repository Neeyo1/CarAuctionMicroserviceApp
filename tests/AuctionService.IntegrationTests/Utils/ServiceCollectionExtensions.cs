using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Utils;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(x => 
            x.ServiceType == typeof(DbContextOptions<AuctionDbContext>));
        if (descriptor != null) services.Remove(descriptor);
    }

    public static void EnsureCreated(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<AuctionDbContext>();

        context.Database.Migrate();

        DbHelper.InitDbForTests(context);
    }
}
