using Microsoft.AspNetCore.Mvc;
using TodoApi.Repositories;
using TodoApi.Models;
namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IBaseRepository<WeatherForecast> _weatherRepository;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IBaseRepository<WeatherForecast> weatherRepository)
    {
        _logger = logger;
        _weatherRepository = weatherRepository;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        return await _weatherRepository.GetAllAsync();
    }
}
