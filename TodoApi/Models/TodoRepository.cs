using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// Repository interface for TodoItem entity.
/// Extends IBaseRepository with domain-specific query methods.
/// Similar to Laravel's Repository pattern with custom query methods.
/// </summary>
public interface ITodoRepository : IBaseRepository<TodoItem>
{
  /// <summary>
  /// Get all todos for with category eagerly loaded.
  /// Similar to Laravel: Todo::all()->with('category')->get()
  /// </summary>
  Task<IEnumerable<TodoItem>> GetTodosWithCategoryAsync();

  /// <summary>
  /// Get a single todo with its category eagerly loaded.
  /// Prevents N+1 query issues.
  /// </summary>
  Task<TodoItem?> GetTodoWithCategoryAsync(int id);

  /// <summary>
  /// Get all todos for a specific category.
  /// Similar to Laravel: Todo::where('category_id', $id)->with('category')->get()
  /// </summary>
  Task<IEnumerable<TodoItem>> GetTodosByCategoryAsync(int categoryId);

  /// <summary>
  /// Get all completed todos.
  /// Similar to Laravel's query scope: Todo::completed()->get()
  /// </summary>
  Task<IEnumerable<TodoItem>> GetCompletedTodosAsync();

  /// <summary>
  /// Get all pending (incomplete) todos.
  /// </summary>
  Task<IEnumerable<TodoItem>> GetPendingTodosAsync();

  /// <summary>
  /// Get all overdue todos (not completed and past due date).
  /// Demonstrates complex filtering logic encapsulated in repository.
  /// </summary>
  Task<IEnumerable<TodoItem>> GetOverdueTodosAsync();
}

/// <summary>
/// Repository implementation for TodoItem.
/// Inherits common CRUD operations from BaseRepository.
/// Adds domain-specific queries with proper eager loading.
/// </summary>
public class TodoRepository : BaseRepository<TodoItem>, ITodoRepository
{
  /// <summary>
  /// Constructor - receives DbContext via dependency injection.
  /// Passes it to base class which sets up DbSet<TodoItem>.
  /// </summary>
  public TodoRepository(AppDbContext context) : base(context)
  {
  }

  public async Task<IEnumerable<TodoItem>> GetTodosWithCategoryAsync()
  {
    // Include() eagerly loads the Category relationship
    // Similar to Laravel: Todo::with('category')->get()
    return await DbSet
        .Include(t => t.Category)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
  }

  public async Task<TodoItem?> GetTodoWithCategoryAsync(int id)
  {
    // DbSet is protected property from BaseRepository
    // Include() eagerly loads the Category relationship
    // Similar to Laravel: Todo::with('category')->find($id)
    return await DbSet
        .Include(t => t.Category)
        .FirstOrDefaultAsync(t => t.Id == id);
  }

  public async Task<IEnumerable<TodoItem>> GetTodosByCategoryAsync(int categoryId)
  {
    // Where() filters by CategoryId (SQL WHERE clause)
    // Include() prevents N+1 queries
    // OrderByDescending() for newest first
    return await DbSet
        .Include(t => t.Category)
        .Where(t => t.CategoryId == categoryId)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
  }

  public async Task<IEnumerable<TodoItem>> GetCompletedTodosAsync()
  {
    // Simple boolean filter
    // This is like Laravel's: Todo::where('is_completed', true)->get()
    return await DbSet
        .Include(t => t.Category)
        .Where(t => t.IsCompleted)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
  }

  public async Task<IEnumerable<TodoItem>> GetPendingTodosAsync()
  {
    // Negation filter: WHERE IsCompleted = false
    return await DbSet
        .Include(t => t.Category)
        .Where(t => !t.IsCompleted)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
  }

  public async Task<IEnumerable<TodoItem>> GetOverdueTodosAsync()
  {
    var now = DateTime.UtcNow;

    // Complex WHERE clause with multiple conditions:
    // 1. Not completed
    // 2. Has a due time (DueTime is not null)
    // 3. Due time is in the past
    //
    // Similar to Laravel:
    // Todo::where('is_completed', false)
    //     ->whereNotNull('due_time')
    //     ->where('due_time', '<', now())
    //     ->orderBy('due_time')
    //     ->get()
    return await DbSet
        .Include(t => t.Category)
        .Where(t => !t.IsCompleted &&           // Not completed
                   t.DueTime.HasValue &&        // Has a due time
                   t.DueTime.Value < now)       // Due time is in the past
        .OrderBy(t => t.DueTime)                // Sort by most overdue first
        .ToListAsync();
  }
}
