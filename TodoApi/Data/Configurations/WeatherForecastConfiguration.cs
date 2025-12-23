using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApi.Models;

namespace TodoApi.Data.Configurations;

/* 
  configurations are a way to separate entity configuration logic from the DbContext.
  this allow handle more complex configurations that not match a convention.

  like a foreing key with custom name, indexes, constraints, etc.
  also help to keep the DbContext clean and focused on its primary responsibility of managing the database
 */
public class WeatherForecastConfiguration : IEntityTypeConfiguration<WeatherForecast>
{
  public void Configure(EntityTypeBuilder<WeatherForecast> builder)
  {
    builder.HasKey(e => e.Id);
    builder.Property(e => e.Summary).HasMaxLength(200);
  }
}
