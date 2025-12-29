using FluentValidation;
using TodoApi.Models;

namespace TodoApi.Requests;

/// <summary>
/// Validator for UpdateTodoItemRequest.
/// All fields are optional, but if provided, they must be valid.
/// Similar to Laravel Form Request validation rules.
/// </summary>
public class UpdateTodoItemRequestValidator : AbstractValidator<UpdateTodoItemRequest>
{
  public UpdateTodoItemRequestValidator()
  {
    // Title validation (optional for update, but if provided must be valid)
    RuleFor(x => x.Title)
        .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
        .MinimumLength(3).WithMessage("Title must be at least 3 characters")
        .When(x => !string.IsNullOrEmpty(x.Title));

    // CategoryId validation (optional for update, but if provided must be valid)
    RuleFor(x => x.CategoryId)
        .GreaterThan(0).WithMessage("CategoryId must be greater than 0")
        .When(x => x.CategoryId.HasValue);
    // Note: Category existence check is done in the controller

    // DueTime validation (optional but must be future date if provided)
    RuleFor(x => x.DueTime)
        .Must(BeAFutureDate).WithMessage("Due time must be in the future")
        .When(x => x.DueTime.HasValue);
  }

  /// <summary>
  /// Custom validation: Check if due time is in the future.
  /// </summary>
  private bool BeAFutureDate(DateTime? dueTime)
  {
    return !dueTime.HasValue || dueTime.Value > DateTime.UtcNow;
  }
}
