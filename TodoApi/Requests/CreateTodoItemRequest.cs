namespace TodoApi.Requests;

/// <summary>
/// Request DTO for creating a new todo item.
/// Similar to Laravel Form Request.
/// </summary>
public class CreateTodoItemRequest
{
  public string Title { get; set; } = string.Empty;
  public int? CategoryId { get; set; }
  public DateTime? DueTime { get; set; }
}
