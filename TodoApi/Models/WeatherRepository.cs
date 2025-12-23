using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

public class WeatherRepository : IBaseRepository<WeatherForecast>
{
  private readonly AppDbContext _context;

  public WeatherRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<WeatherForecast>> GetAllAsync()
  {
    return await _context.WeatherForecasts.ToListAsync();
  }
}