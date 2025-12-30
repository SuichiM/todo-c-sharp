using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Resources;

namespace TodoApi.Controllers;

/// <summary>
/// Categories API Controller.
/// Provides endpoints to retrieve category information for organizing todo items.
/// Similar to Laravel's resource controller but simplified to only include needed endpoints.
/// </summary>
[ApiController]
[Route("api/categories")]  // Results in: /api/categories
public class CategoriesController : ControllerBase
{
  private readonly IBaseRepository<Category> _categoryRepository;
  private readonly ILogger<CategoriesController> _logger;

  /// <summary>
  /// Constructor injection using the generic IBaseRepository interface.
  /// The repository is already registered in Program.cs as CategoryRepository.
  /// Similar to Laravel's dependency injection in controllers.
  /// </summary>
  public CategoriesController(
      IBaseRepository<Category> categoryRepository,
      ILogger<CategoriesController> logger)
  {
    _categoryRepository = categoryRepository;
    _logger = logger;
  }

  /// <summary>
  /// GET: api/categories
  /// Retrieves all categories available in the system.
  /// Used by the frontend to populate category selection dropdowns.
  /// 
  /// Returns:
  /// - 200 OK with array of categories (even if empty)
  /// - 500 Internal Server Error if database query fails
  /// </summary>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
  {
    try
    {
      _logger.LogInformation("Fetching all categories");

      var categories = await _categoryRepository.GetAllAsync();

      _logger.LogInformation("Successfully retrieved {Count} categories", categories.Count());

      // Return 200 OK with the categories
      // In ASP.NET Core, returning a collection automatically serializes it to JSON
      // Similar to Laravel: return response()->json($categories);
      return Ok(CategoryDto.Collection(categories));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving categories");

      // Return 500 Internal Server Error
      // In production, you might want to return a more user-friendly error message
      return StatusCode(StatusCodes.Status500InternalServerError,
          new { message = "An error occurred while retrieving categories" });
    }
  }
}
