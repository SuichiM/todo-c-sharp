using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

/// <summary>
/// TodoItems API Controller - Educational version using direct DbContext.
/// This demonstrates how EF Core queries work before abstracting to repository pattern.
/// Similar to Laravel's approach: Query Builder → Eloquent → Repository
/// </summary>
[ApiController]
[Route("api/todos")]  // Results in: /api/todos
public class TodoItemsController : ControllerBase
{
  private readonly AppDbContext _context;
  private readonly ILogger<TodoItemsController> _logger;

  /// <summary>
  /// Constructor injection - ASP.NET Core's built-in dependency injection
  /// Similar to Laravel's constructor injection
  /// </summary>
  public TodoItemsController(AppDbContext context, ILogger<TodoItemsController> logger)
  {
    _context = context;
    _logger = logger;
  }

  /// <summary>
  /// GET: api/todos
  /// Retrieves all todo items with their categories.
  /// </summary>
  /// <returns>List of all todo items</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
  {
    // EDUCATIONAL POINT: Direct DbContext usage
    // _context.Set<TodoItem>() gets the DbSet for TodoItem entity
    // This is how BaseRepository accesses entities dynamically

    // ⚠️ N+1 QUERY PROBLEM EXAMPLE (DON'T DO THIS):
    // var todos = await _context.Set<TodoItem>().ToListAsync();
    // Accessing todo.Category later would trigger individual queries for each todo!

    // ✅ CORRECT APPROACH: Eager loading with Include()
    var todos = await _context.Set<TodoItem>()
        .Include(t => t.Category)           // LEFT JOIN with Categories table
        .OrderByDescending(t => t.CreatedAt) // ORDER BY CreatedAt DESC
        .ToListAsync();                      // Execute query and materialize results

    // LINQ Translation: This becomes approximately:
    // SELECT t.*, c.* 
    // FROM TodoItems t 
    // LEFT JOIN Categories c ON t.CategoryId = c.Id
    // ORDER BY t.CreatedAt DESC
    //
    // Because Category is nullable (Category? and int? CategoryId),
    // EF Core generates a LEFT JOIN, allowing todos without categories.

    _logger.LogInformation("Retrieved {Count} todo items", todos.Count);

    return Ok(todos);  // Returns 200 OK with JSON body
  }

  /// <summary>
  /// GET: api/TodoItems/5
  /// Retrieves a specific todo item by ID.
  /// </summary>
  /// <param name="id">The ID of the todo item</param>
  /// <returns>The todo item if found</returns>
  [HttpGet("{id}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
  {
    // FirstOrDefaultAsync returns the first match or null
    // Similar to Laravel's Model::where('id', $id)->first()
    var todo = await _context.Set<TodoItem>()
        .Include(t => t.Category)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (todo == null)
    {
      _logger.LogWarning("Todo item {Id} not found", id);

      // NotFound() returns 404 status with optional body
      return NotFound(new { message = $"Todo item with ID {id} not found" });
    }

    return Ok(todo);  // 200 OK with todo data
  }

  /// <summary>
  /// GET: api/TodoItems/category/2
  /// Retrieves all todos for a specific category.
  /// </summary>
  /// <param name="categoryId">The ID of the category</param>
  /// <returns>List of todos in the category</returns>
  [HttpGet("category/{categoryId}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodosByCategory(int categoryId)
  {
    // Check if category exists first
    // AnyAsync is efficient - just checks existence without loading data
    // Similar to Laravel's Model::exists()
    var categoryExists = await _context.Set<Category>()
        .AnyAsync(c => c.Id == categoryId);

    if (!categoryExists)
    {
      return NotFound(new { message = $"Category with ID {categoryId} not found" });
    }

    // Query todos for this category
    // Where() adds a filter condition (WHERE clause in SQL)
    var todos = await _context.Set<TodoItem>()
        .Include(t => t.Category)
        .Where(t => t.CategoryId == categoryId)  // WHERE CategoryId = @categoryId
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

    _logger.LogInformation(
        "Retrieved {Count} todos for category {CategoryId}",
        todos.Count,
        categoryId);

    return Ok(todos);
  }

  /// <summary>
  /// GET: api/TodoItems/completed
  /// Retrieves all completed todos.
  /// </summary>
  [HttpGet("completed")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TodoItem>>> GetCompletedTodos()
  {
    // Filtering by IsCompleted status
    var todos = await _context.Set<TodoItem>()
        .Include(t => t.Category)
        .Where(t => t.IsCompleted)  // WHERE IsCompleted = true
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

    return Ok(todos);
  }

  /// <summary>
  /// GET: api/TodoItems/pending
  /// Retrieves all pending (incomplete) todos.
  /// </summary>
  [HttpGet("pending")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TodoItem>>> GetPendingTodos()
  {
    var todos = await _context.Set<TodoItem>()
        .Include(t => t.Category)
        .Where(t => !t.IsCompleted)  // WHERE IsCompleted = false
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

    return Ok(todos);
  }

  /// <summary>
  /// GET: api/TodoItems/overdue
  /// Retrieves all overdue todos (not completed and past due time).
  /// </summary>
  [HttpGet("overdue")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TodoItem>>> GetOverdueTodos()
  {
    var now = DateTime.UtcNow;

    // Complex WHERE clause with multiple conditions
    var todos = await _context.Set<TodoItem>()
        .Include(t => t.Category)
        .Where(t => !t.IsCompleted &&           // Not completed
                   t.DueTime.HasValue &&        // Has a due time
                   t.DueTime.Value < now)       // Due time is in the past
        .OrderBy(t => t.DueTime)                // Sort by most overdue first
        .ToListAsync();

    _logger.LogInformation("Found {Count} overdue todos", todos.Count);

    return Ok(todos);
  }
}
