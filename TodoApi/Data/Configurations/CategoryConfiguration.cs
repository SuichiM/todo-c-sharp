using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApi.Models;

namespace TodoApi.Data.Configurations;

/// <summary>
/// Fluent API configuration for Category entity.
/// This approach keeps the entity class clean (POCO) and centralizes database configuration.
/// Similar to Laravel's Schema Builder, but for entity relationships and constraints.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    // Primary Key configuration
    builder.HasKey(c => c.Id);

    // Table name (optional - EF Core would use "Categories" by default)
    builder.ToTable("Categories");

    // Property configurations
    builder.Property(c => c.Name)
        .IsRequired()                    // NOT NULL constraint
        .HasMaxLength(100);              // VARCHAR(100)

    // Index for performance - categories will be searched/filtered by name
    // Similar to Laravel's $table->index('name')
    builder.HasIndex(c => c.Name)
        .HasDatabaseName("IX_Categories_Name");

    // Relationship configuration: One Category has many TodoItems
    // This is one side of the one-to-many relationship
    builder.HasMany(c => c.TodoItems)           // Category has many TodoItems
        .WithOne(t => t.Category)                // Each TodoItem has one Category
        .HasForeignKey(t => t.CategoryId)        // Foreign key is TodoItem.CategoryId
        .OnDelete(DeleteBehavior.Restrict);      // Prevent cascade delete (safety)

    // DeleteBehavior.Restrict means:
    // - Cannot delete a Category if it has TodoItems
    // - Must delete/reassign TodoItems first
    // - Similar to Laravel's ->onDelete('restrict')
  }
}
