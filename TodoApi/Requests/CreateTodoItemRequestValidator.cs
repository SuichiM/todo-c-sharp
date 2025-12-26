using FluentValidation;
using TodoApi.Repositories;
using TodoApi.Models;

namespace TodoApi.Requests;

/// <summary>
/// Validator for CreateTodoItemRequest.
/// Similar to Laravel Form Request validation rules.
/// </summary>
public class CreateTodoItemRequestValidator : AbstractValidator<CreateTodoItemRequest>
{
  private readonly IBaseRepository<Category> _categoryRepository;

  public CreateTodoItemRequestValidator(IBaseRepository<Category> categoryRepository)
  {
    _categoryRepository = categoryRepository;

    // Title validation
    RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Title is required")
        .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
        .MinimumLength(3).WithMessage("Title must be at least 3 characters");

    // CategoryId validation
    RuleFor(x => x.CategoryId)
        .NotNull().WithMessage("CategoryId is required")
        .GreaterThan(0).WithMessage("CategoryId must be greater than 0");
    // Note: Category existence check is done in the controller
    // Async validators don't work with ASP.NET automatic validation

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

