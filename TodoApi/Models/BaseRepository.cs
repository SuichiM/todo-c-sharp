using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Repositories;

/// <summary>
/// Generic repository implementation using DbContext.Set<T>() for runtime entity access.
/// Provides common CRUD operations for any entity type configured in EF Core.
/// </summary>
/// <typeparam name="T">Entity type that must be a reference type (class)</typeparam>
public class BaseRepository<T> : IBaseRepository<T> where T : class
{
  protected readonly AppDbContext Context;
  protected readonly DbSet<T> DbSet;

  public BaseRepository(AppDbContext context)
  {
    Context = context;
    // Use Set<T>() to get DbSet for any entity type at runtime
    DbSet = context.Set<T>();
  }

  public virtual async Task<IEnumerable<T>> GetAllAsync()
  {
    return await DbSet.ToListAsync();
  }
}
