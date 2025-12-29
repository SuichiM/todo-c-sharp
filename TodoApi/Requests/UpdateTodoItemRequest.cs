namespace TodoApi.Requests;

/// <summary>
/// Request DTO for updating a todo item.
/// All fields are optional for partial updates.
/// Similar to Laravel Form Request.
/// </summary>
public class UpdateTodoItemRequest
{
  public string? Title { get; set; }
  public bool? IsCompleted { get; set; }
  public int? CategoryId { get; set; }
  public DateTime? DueTime { get; set; }
}
