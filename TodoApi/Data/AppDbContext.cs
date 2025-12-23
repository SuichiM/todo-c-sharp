using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  /* TODO: check if this should be done for all the models, or could we have something generic */
  public DbSet<WeatherForecast> WeatherForecasts { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Configure WeatherForecast entity
    modelBuilder.Entity<WeatherForecast>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Summary).HasMaxLength(200);
    });
  }
}
