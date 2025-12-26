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

  /// <summary>
  /// Add a new entity to the database.
  /// Similar to Laravel: Model::create($data)
  /// </summary>
  Task<T> AddAsync(T entity);

  /// <summary>
  /// Update an existing entity.
  /// Similar to Laravel: $model->update($data)
  /// </summary>
  Task<T> UpdateAsync(T entity);

  /// <summary>
  /// Delete an entity by ID.
  /// Similar to Laravel: Model::destroy($id)
  /// </summary>
  Task<bool> DeleteAsync(int id);
}
