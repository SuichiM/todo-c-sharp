using System.Linq.Expressions;

namespace TodoApi.Repositories;

public interface IBaseRepository<T> where T : class
{
  // Core methods - must implement
  Task<IEnumerable<T>> GetAllAsync();
}
