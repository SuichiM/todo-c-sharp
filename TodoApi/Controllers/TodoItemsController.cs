using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Resources;
using TodoApi.Repositories;
using TodoApi.Requests;

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
  /// POST: api/todos
  /// Creates a new todo item.
  /// FluentValidation automatically validates the request before this method executes.
  /// If validation fails, returns 400 Bad Request with validation errors.
  /// </summary>
  /// <param name="request">The todo item creation data</param>
  /// <returns>The created todo item with 201 Created status</returns>
  [HttpPost]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<TodoItemDto>> CreateTodoItem(CreateTodoItemRequest request)
  {
    // At this point, FluentValidation has already validated:
    // 1. Title (required, min 3, max 200 chars)
    // 2. CategoryId (not null, > 0)
    // 3. DueTime (must be future date if provided)
    //
    // Similar to Laravel Form Request validation

    // Check if category exists (async validation not supported in automatic validation)
    if (!await _categoryRepository.ExistsAsync(request.CategoryId!.Value))
    {
      return BadRequest(new { message = "Category does not exist" });
    }

    // Map request to entity
    var todoItem = new TodoItem
    {
      Title = request.Title,
      CategoryId = request.CategoryId.Value,
      DueTime = request.DueTime,
      CreatedAt = DateTime.UtcNow,
      IsCompleted = false
    };

    // Save to database via repository
    var created = await _todoRepository.AddAsync(todoItem);

    // Load the entity with category relationship for response
    var todoWithCategory = await _todoRepository.GetTodoWithCategoryAsync(created.Id);

    _logger.LogInformation("Created todo item {Id}: {Title}", created.Id, created.Title);

    // Return 201 Created with Location header
    // Location header: /api/todos/5
    // Similar to Laravel: return response()->json($todo, 201)->header('Location', ...)
    return CreatedAtAction(
      nameof(GetTodoItem),
      /*TODO: review then, what is this kind of anonymous object */
      new { id = created.Id },
      TodoItemDto.Make(todoWithCategory!)
    );
  }

  /// <summary>
  /// PUT: api/todos/5
  /// Updates an existing todo item.
  /// Supports partial updates - only provided fields are updated.
  /// </summary>
  /// <param name="id">The ID of the todo item to update</param>
  /// <param name="request">The update data</param>
  /// <returns>The updated todo item</returns>
  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<TodoItemDto>> UpdateTodoItem(int id, UpdateTodoItemRequest request)
  {
    // FluentValidation has already validated:
    // 1. Title (if provided: min 3, max 200 chars)
    // 2. CategoryId (if provided: > 0)
    // 3. DueTime (if provided: must be future date)

    // Check if todo exists
    // TODO: review why this var hadn't type 
    var existingTodo = await _todoRepository.GetByIdAsync(id);
    if (existingTodo == null)
    {
      return NotFound(new { message = $"Todo item with ID {id} not found" });
    }

    // If category is being updated, check if it exists
    if (request.CategoryId.HasValue)
    {
      if (!await _categoryRepository.ExistsAsync(request.CategoryId.Value))
      {
        return BadRequest(new { message = "Category does not exist" });
      }
    }

    // Apply partial updates (only update provided fields)
    if (!string.IsNullOrEmpty(request.Title))
      existingTodo.Title = request.Title;

    if (request.IsCompleted.HasValue)
      existingTodo.IsCompleted = request.IsCompleted.Value;

    if (request.CategoryId.HasValue)
      existingTodo.CategoryId = request.CategoryId.Value;

    if (request.DueTime.HasValue)
      existingTodo.DueTime = request.DueTime;

    // Save changes
    var updated = await _todoRepository.UpdateAsync(existingTodo);

    // Load with category for response
    var todoWithCategory = await _todoRepository.GetTodoWithCategoryAsync(updated.Id);

    _logger.LogInformation("Updated todo item {Id}", id);

    return Ok(TodoItemDto.Make(todoWithCategory!));
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
