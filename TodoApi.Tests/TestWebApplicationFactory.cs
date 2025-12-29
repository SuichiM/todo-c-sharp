using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;

namespace TodoApi.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Replaces PostgreSQL with in-memory SQLite for isolated, fast tests.
/// Similar to Laravel's RefreshDatabase trait but for ASP.NET.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing to prevent PostgreSQL registration
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptorDbContext = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptorDbContext != null)
            {
                services.Remove(descriptorDbContext);
            }

            // Also remove the DbContext itself
            var descriptorContext = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext));

            if (descriptorContext != null)
            {
                services.Remove(descriptorContext);
            }

            // Create and open a connection to SQLite in-memory database
            // Keep it open for the lifetime of the test to prevent tables from being dropped
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add DbContext with in-memory SQLite
            // Using SQLite instead of InMemory provider because:
            // - SQLite enforces constraints (foreign keys, unique indexes)
            // - SQLite is closer to real database behavior
            // - InMemory provider is too lenient for realistic testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create database schema
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                // Create schema from entity configurations
                db.Database.EnsureCreated();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
