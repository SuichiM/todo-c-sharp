using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApi.Models;

namespace TodoApi.Data.Configurations;

/// <summary>
/// Fluent API configuration for TodoItem entity.
/// Defines database schema, indexes, and the relationship with Category.
/// </summary>
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // Primary Key
        builder.HasKey(t => t.Id);

        // Table name (optional - EF Core would use "TodoItems" by default)
        builder.ToTable("TodoItems");

        // Property configurations with constraints
        builder.Property(t => t.Title)
            .IsRequired()                    // NOT NULL
            .HasMaxLength(200);              // VARCHAR(200)

        builder.Property(t => t.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);         // Default to false for new todos

        builder.Property(t => t.CreatedAt)
            .IsRequired()                   // NOT NULL - every todo must have creation timestamp   
            .HasDefaultValueSql("current_timestamp");
        builder.Property(t => t.DueTime)
            .IsRequired(false);              // Nullable - due time is optional

        // Indexes for query performance
        // These columns will frequently be used in WHERE clauses and JOINs

        // Index on CategoryId (foreign key) - speeds up category filtering and joins
        // EF Core often creates this automatically, but explicit is clearer
        builder.HasIndex(t => t.CategoryId)
            .HasDatabaseName("IX_TodoItems_CategoryId");

        // Index on CreatedAt - for sorting and date range queries
        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("IX_TodoItems_CreatedAt");

        // Index on IsCompleted - for filtering completed/pending todos
        builder.HasIndex(t => t.IsCompleted)
            .HasDatabaseName("IX_TodoItems_IsCompleted");

        // Composite index for common query pattern: filter by category and completion status
        builder.HasIndex(t => new { t.CategoryId, t.IsCompleted })
            .HasDatabaseName("IX_TodoItems_CategoryId_IsCompleted");

        // Relationship configuration: Many TodoItems belong to one Category
        // This is the inverse side of the relationship (already defined in CategoryConfiguration)
        // Defining from both sides is optional but makes the intent explicit
        builder.HasOne(t => t.Category)              // TodoItem has one Category
            .WithMany(c => c.TodoItems)              // Category has many TodoItems
            .HasForeignKey(t => t.CategoryId)        // Foreign key column
            .OnDelete(DeleteBehavior.Restrict)      // Cannot delete category with todos
            .IsRequired(false);  // 🔑 Changed from .IsRequired() to .IsRequired(false)

        // Note: DeleteBehavior.Restrict protects data integrity
        // If you want automatic cleanup, use DeleteBehavior.Cascade
        // Similar to Laravel's ->onDelete('cascade') vs ->onDelete('restrict')
    }
}
