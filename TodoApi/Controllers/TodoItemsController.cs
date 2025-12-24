using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.DTOs;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

/// <summary>
/// TodoItems API Controller - Now using Repository Pattern.
/// Controller focuses on HTTP concerns, repositories handle data access.
/// Similar to Laravel's best practice: Controller → Repository → Model
/// </summary>
[ApiController]
[Route("api/todos")]  // Results in: /api/todos
public class TodoItemsController : ControllerBase
{
  private readonly ITodoRepository _todoRepository;
  private readonly IBaseRepository<Category> _categoryRepository;
  private readonly ILogger<TodoItemsController> _logger;

  /// <summary>
  /// Constructor injection - Now using repositories instead of DbContext.
  /// Repositories abstract away EF Core details, making controller cleaner and more testable.
  /// Similar to Laravel's repository pattern injection.
  /// </summary>
  public TodoItemsController(
      ITodoRepository todoRepository,
      IBaseRepository<Category> categoryRepository,
      ILogger<TodoItemsController> logger)
  {
    _todoRepository = todoRepository;
    _categoryRepository = categoryRepository;
    _logger = logger;
  }

  /// <summary>
  /// GET: api/todos
  /// Retrieves all todo items with their categories.
  /// </summary>
  /// <returns>List of all todo items</returns>
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodoItems()
  {
    // REPOSITORY PATTERN: Much cleaner than direct DbContext!
    // Repository handles:
    // - EF Core query construction
    // - Eager loading (Include)
    // - Ordering logic
    //
    // Controller just asks for data and returns it
    // Similar to Laravel: $todos = $this->todoRepository->getAll();
    var todos = await _todoRepository.GetTodosWithCategoryAsync();

    _logger.LogInformation("Retrieved {Count} todo items", todos.Count());

    return Ok(TodoItemDto.Collection(todos));
  }

  /// <summary>
  /// GET: api/todos/5
  /// Retrieves a specific todo item by ID.
  /// </summary>
  /// <param name="id">The ID of the todo item</param>
  /// <returns>The todo item if found</returns>
  [HttpGet("{id}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<TodoItemDto>> GetTodoItem(int id)
  {
    // Repository method handles eager loading automatically
    // No need to worry about Include() in controller
    var todo = await _todoRepository.GetTodoWithCategoryAsync(id);

    if (todo == null)
    {
      _logger.LogWarning("Todo item {Id} not found", id);
      return NotFound(new { message = $"Todo item with ID {id} not found" });
    }

    return Ok(TodoItemDto.Make(todo));
  }

  /// <summary>
  /// GET: api/todos/category/2
  /// Retrieves all todos for a specific category.
  /// </summary>
  /// <param name="categoryId">The ID of the category</param>
  /// <returns>List of todos in the category</returns>
  [HttpGet("category/{categoryId}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodosByCategory(int categoryId)
  {
    // Use ExistsAsync - more efficient than loading the entire entity
    // Similar to Laravel: if (!Category::where('id', $id)->exists())
    if (!await _categoryRepository.ExistsAsync(categoryId))
    {
      return NotFound(new { message = $"Category with ID {categoryId} not found" });
    }

    // Repository method encapsulates the query logic
    var todos = await _todoRepository.GetTodosByCategoryAsync(categoryId);

    _logger.LogInformation(
        "Retrieved {Count} todos for category {CategoryId}",
        todos.Count(),
        categoryId);

    return Ok(TodoItemDto.Collection(todos));
  }

  /// <summary>
  /// GET: api/todos/completed
  /// Retrieves all completed todos.
  /// </summary>
  [HttpGet("completed")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetCompletedTodos()
  {
    // Repository encapsulates filtering logic
    var todos = await _todoRepository.GetCompletedTodosAsync();

    return Ok(TodoItemDto.Collection(todos));
  }

  /// <summary>
  /// GET: api/todos/pending
  /// Retrieves all pending (incomplete) todos.
  /// </summary>
  [HttpGet("pending")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetPendingTodos()
  {
    var todos = await _todoRepository.GetPendingTodosAsync();

    return Ok(TodoItemDto.Collection(todos));
  }

  /// <summary>
  /// GET: api/todos/overdue
  /// Retrieves all overdue todos (not completed and past due time).
  /// </summary>
  [HttpGet("overdue")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetOverdueTodos()
  {
    // Repository encapsulates complex filtering logic
    var todos = await _todoRepository.GetOverdueTodosAsync();

    _logger.LogInformation("Found {Count} overdue todos", todos.Count());

    return Ok(TodoItemDto.Collection(todos));
  }
}
