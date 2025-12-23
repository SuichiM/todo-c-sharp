using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  // DbSet properties are optional - EF Core will discover entities through configurations
  // However, keeping them provides better IntelliSense and makes querying more convenient
  public DbSet<WeatherForecast> WeatherForecasts { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Automatically apply all IEntityTypeConfiguration implementations from this assembly
    // This eliminates the need to manually configure each entity here
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
  }
}
