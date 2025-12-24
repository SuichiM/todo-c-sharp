using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// Category repository implementation.
/// Uses only base repository methods - no custom queries needed.
/// Similar to Laravel: just using the base Eloquent methods without a custom repository.
/// </summary>
public class CategoryRepository : BaseRepository<Category>
{
  public CategoryRepository(AppDbContext context) : base(context)
  {
  }
  
  // That's it! All methods inherited from BaseRepository:
  // - GetAllAsync()
  // - GetByIdAsync(id)
  // - ExistsAsync(id)
}