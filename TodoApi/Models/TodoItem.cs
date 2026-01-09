namespace TodoApi.Models;

/// <summary>
/// Represents a todo item with an explicit foreign key relationship to Category.
/// This follows the recommended pattern in EF Core for clarity and control.
/// </summary>
public class TodoItem
{
  public int Id { get; set; }

  public string Title { get; set; } = string.Empty;

  public bool IsCompleted { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime? DueTime { get; set; }

  // Explicit Foreign Key - gives you direct access to the ID without loading the entity
  // Similar to Laravel's foreign key in migrations, but defined in the model
  public int? CategoryId { get; set; }

  // Navigation property - tells EF Core about the relationship
  // Nullable (?) allows EF Core flexibility during queries
  // Similar to Laravel's belongsTo() relationship
  public Category? Category { get; set; }

  // Maps to a text[] column in PostgreSQL
  public List<string>? Tags { get; set; } = new List<string>();
}
