using TodoApi.Models;

namespace TodoApi.DTOs;

/// <summary>
/// TodoItem Data Transfer Object
/// Defines the shape of TodoItem in API responses
/// Similar to Laravel's TodoItemResource
/// </summary>
public class TodoItemDto
{
  public int Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public bool IsCompleted { get; set; }
  public DateTime? DueTime { get; set; }
  public DateTime CreatedAt { get; set; }

  // Nested category info
  public CategoryDto? Category { get; set; }

  /// <summary>
  /// Transform a single TodoItem to DTO
  /// Usage: TodoItemDto.Make(todo)
  /// Similar to Laravel: new TodoItemResource($todo)
  /// </summary>
  public static TodoItemDto Make(TodoItem todo)
  {
    return new TodoItemDto
    {
      Id = todo.Id,
      Title = todo.Title,
      IsCompleted = todo.IsCompleted,
      DueTime = todo.DueTime,
      CreatedAt = todo.CreatedAt,
      Category = todo.Category == null ? null : new CategoryDto
      {
        Id = todo.Category.Id,
        Name = todo.Category.Name
      }
    };
  }

  /// <summary>
  /// Transform a collection of TodoItems to DTOs
  /// Usage: TodoItemDto.Collection(todos)
  /// Similar to Laravel: TodoItemResource::collection($todos)
  /// </summary>
  public static List<TodoItemDto> Collection(IEnumerable<TodoItem> todos)
  {
    return todos.Select(t => Make(t)).ToList();
  }
}

/// <summary>
/// Category Data Transfer Object
/// </summary>
public class CategoryDto
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
}

