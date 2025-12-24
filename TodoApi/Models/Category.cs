namespace TodoApi.Models;

/// <summary>
/// Represents a category that can contain multiple todo items.
/// Demonstrates one-to-many relationship in EF Core.
/// </summary>
public class Category
{
  public int Id { get; set; }

  public string Name { get; set; } = string.Empty;

  // Collection navigation property - defines the "one-to-many" relationship
  // Initialized to empty list to prevent null reference exceptions
  // Similar to Laravel's hasMany() relationship
  public List<TodoItem> TodoItems { get; set; } = new();
}
