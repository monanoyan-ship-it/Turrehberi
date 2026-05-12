using ErkanTatilPlani.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ErkanTatilPlani.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureServices(services =>
        {
            var databaseName = "TestDb_" + Guid.NewGuid().ToString("N");

            // Remove ALL EF Core related services to avoid provider conflicts
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(AppDbContext));

            // Remove any IDbContextFactory registrations
            var efServices = services
                .Where(d => d.ServiceType.FullName != null &&
                           (d.ServiceType.FullName.Contains("EntityFrameworkCore") ||
                            d.ServiceType.FullName.Contains("Npgsql")))
                .ToList();
            foreach (var svc in efServices)
                services.Remove(svc);

            // Add fresh InMemory database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });
        });

        builder.UseEnvironment("Testing");
    }
}
