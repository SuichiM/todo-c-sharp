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

  /// <summary>
  /// Find entity by primary key (assumes Id property).
  /// FindAsync is optimized for primary key lookups.
  /// </summary>
  public virtual async Task<T?> GetByIdAsync(int id)
  {
    return await DbSet.FindAsync(id);
  }

  /// <summary>
  /// Check if entity exists without loading it.
  /// More efficient than GetByIdAsync when you only need to check existence.
  /// </summary>
  public virtual async Task<bool> ExistsAsync(int id)
  {
    var entity = await GetByIdAsync(id);
    return entity != null;
  }
}
