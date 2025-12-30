using TodoApi.Models;

namespace TodoApi.Resources;

/// <summary>
/// Category Data Transfer Object
/// </summary>
public class CategoryDto
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;

  public static CategoryDto Make(Category category)
  {
    return new CategoryDto
    {
      Id = category.Id,
      Name = category.Name
    };
  }

  public static List<CategoryDto> Collection(IEnumerable<Category> categories)
  {
    return categories.Select(c => Make(c)).ToList();
  }

}
