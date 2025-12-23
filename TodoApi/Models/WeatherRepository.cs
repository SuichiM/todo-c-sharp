using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// Weather-specific repository. Inherits common CRUD operations from BaseRepository.
/// Add weather-specific query methods here as needed.
/// </summary>
public class WeatherRepository : BaseRepository<WeatherForecast>
{
  public WeatherRepository(AppDbContext context) : base(context)
  {
  }

  // Add weather-specific methods here, for example:
  // public async Task<IEnumerable<WeatherForecast>> GetByTemperatureRangeAsync(int min, int max)
  // {
  //   return await DbSet.Where(w => w.TemperatureC >= min && w.TemperatureC <= max).ToListAsync();
  // }
}