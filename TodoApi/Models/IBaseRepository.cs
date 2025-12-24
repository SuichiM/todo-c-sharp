using System.Linq.Expressions;

namespace TodoApi.Repositories;

public interface IBaseRepository<T> where T : class
{
  // Core methods - must implement
  Task<IEnumerable<T>> GetAllAsync();

  /// <summary>
  /// Get entity by ID.
  /// Similar to Laravel: Model::find($id)
  /// </summary>
  Task<T?> GetByIdAsync(int id);

  /// <summary>
  /// Check if entity with given ID exists.
  /// Similar to Laravel: Model::where('id', $id)->exists()
  /// </summary>
  Task<bool> ExistsAsync(int id);
}
