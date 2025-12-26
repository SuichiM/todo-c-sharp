# TODO API with Categories - Implementation Plan

## Progress Checklist

- [x] **Phase 1: Models & Database Schema** ✅
  - [x] Create Model Classes (TodoItem, Category)
  - [x] Configure Relationships with Fluent API
  - [x] Generate and Apply Migrations
- [x] **Phase 2: Controllers with Direct DbContext** ✅
  - [x] Create TodoItemsController with DbContext
  - [x] Implement GET endpoints with eager loading
  - [x] Create and use DTOs for responses
- [x] **Phase 3: Repository Pattern Implementation** ✅
  - [x] Create Repository Classes (TodoRepository, CategoryRepository)
  - [x] Add GetByIdAsync and ExistsAsync to BaseRepository
  - [x] Register Repositories in DI
  - [x] Refactor Controller to Use Repository
- [ ] **Phase 4: Request DTOs and Validation with FluentValidation**
  - [x] Install FluentValidation
  - [x] Create Request DTOs in `Requests/` folder
  - [x] Create Validators co-located with Request DTOs
  - [x] Register FluentValidation
  - [x] Implement POST endpoint for TODO creation
  - [ ] (Future) Create Update and Delete requests with validators
- [ ] **Phase 5: Full CRUD Operations**
  - [ ] Expand BaseRepository with Create/Update/Delete
  - [ ] Complete TodoItemsController CRUD
  - [ ] Create CategoriesController
- [ ] **Phase 6: Testing and Documentation**
  - [ ] Add Unit Tests
  - [ ] Add Integration Tests
  - [ ] Enhance Swagger Documentation

---

## Learning Objectives

This plan will guide you through building a REST API for TODO management with categories using ASP.NET Core 9. You'll learn:

- **Entity Framework Core relationships**: One-to-many relationships with explicit foreign keys
- **Migration workflow**: Model-first approach (vs Laravel's migration-first)
- **Repository pattern**: Abstracting data access for testability and maintainability
- **Controller patterns**: Building RESTful endpoints with proper HTTP semantics
- **DTOs and validation**: Using FluentValidation for request validation (similar to Laravel Form Requests)

## Project Context

**Current Setup:**

- ASP.NET Core 9 with PostgreSQL (Npgsql)
- Entity Framework Core 9
- Generic Repository Pattern (`BaseRepository<T>`) already implemented
- Automatic entity configuration discovery via `IEntityTypeConfiguration<T>`
- Auto-migration in development mode

---

## Phase 1: Models & Database Schema

### Step 1: Create Model Classes (POCOs)

**File:** `TodoApi/Models/TodoItem.cs`

```csharp
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueTime { get; set; }

    // Explicit Foreign Key (recommended in .NET)
    public int CategoryId { get; set; }

    // Navigation property (nullable for EF Core flexibility)
    public Category? Category { get; set; }
}
```

**File:** `TodoApi/Models/Category.cs`

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Collection navigation property (one-to-many)
    public List<TodoItem> TodoItems { get; set; } = new();
}
```

**Key Learning Points:**

1. **Explicit FK Pattern**: `CategoryId` + `Category` navigation property

   - In Laravel/Eloquent: Often implicit via conventions
   - In EF Core: Explicit FK gives you control and clarity
   - Allows you to work with just the ID without loading the entire related entity

2. **Navigation Properties**: Tell EF Core about relationships

   - `Category?` (nullable reference) on TodoItem = "belongs to"
   - `List<TodoItem>` on Category = "has many"
   - Similar to Laravel's `belongsTo()` and `hasMany()`

3. **Default Values**: `= string.Empty` and `= new()` prevent null warnings
   - C# 9+ nullable reference types help catch null-related bugs at compile time
   - Better than Laravel's `$fillable` for type safety

### Step 2: Configure Relationships with Fluent API

**Why Fluent API over Data Annotations?**

- Keeps models clean (pure POCOs, no framework coupling)
- More powerful (can express complex relationships)
- Similar to Laravel's Schema Builder but for relationships too
- Centralized configuration (like Laravel migrations)

**File:** `TodoApi/Data/Configurations/CategoryConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApi.Models;

namespace TodoApi.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Primary Key
        builder.HasKey(c => c.Id);

        // Table name (optional, defaults to DbSet property name)
        builder.ToTable("Categories");

        // Property configurations
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Index for performance (like Laravel's ->index())
        builder.HasIndex(c => c.Name);

        // Relationship: One Category has many TodoItems
        builder.HasMany(c => c.TodoItems)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
    }
}
```

**File:** `TodoApi/Data/Configurations/TodoItemConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApi.Models;

namespace TodoApi.Data.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // Primary Key
        builder.HasKey(t => t.Id);

        // Table name
        builder.ToTable("TodoItems");

        // Property configurations
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.DueTime)
            .IsRequired(false); // Nullable

        // Indexes for common queries
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.IsCompleted);

        // Relationship: Many TodoItems belong to one Category
        // (Already defined from Category side, but you can define from both)
        builder.HasOne(t => t.Category)
            .WithMany(c => c.TodoItems)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

**Key Learning Points:**

1. **`IEntityTypeConfiguration<T>`**: Auto-discovered by EF Core

   - Your `AppDbContext` uses `modelBuilder.ApplyConfigurationsFromAssembly()`
   - No need to manually register each configuration
   - Similar to Laravel auto-discovering migrations

2. **Relationship Configuration**: Can be defined from either side

   - `HasMany().WithOne()` from Category side
   - `HasOne().WithMany()` from TodoItem side
   - Both specify the same relationship, just different perspectives

3. **Delete Behavior Options**:

   - `Restrict`: Prevents deletion if related records exist (safest)
   - `Cascade`: Deletes related records (like Laravel's `onDelete('cascade')`)
   - `SetNull`: Sets FK to null on delete
   - `NoAction`: Database handles it

4. **Indexes**: Improve query performance
   - Add indexes on FKs (EF Core often auto-creates these)
   - Add indexes on frequently filtered columns (`IsCompleted`, `CreatedAt`)

### Step 3: Generate Migration

**Command:**

```bash
dotnet ef migrations add AddTodoAndCategory
```

**What Happens:**

1. EF Core compares your current model with the last migration snapshot
2. Generates three files in `TodoApi/Migrations/`:
   - `{timestamp}_AddTodoAndCategory.cs` - Up/Down methods
   - `{timestamp}_AddTodoAndCategory.Designer.cs` - Metadata
   - `AppDbContextModelSnapshot.cs` - Updated snapshot (like Laravel's schema cache)

**Key Learning: Model-First Approach**

- **Laravel**: Write migration → run migration → schema is SoT
- **.NET**: Change models → generate migration → models are SoT
- Migrations are "database evolution history", not the design

**Review Generated SQL:**

```bash
dotnet ef migrations script
```

**Expected SQL Output:**

```sql
-- Create Categories table
CREATE TABLE "Categories" (
    "Id" SERIAL PRIMARY KEY,
    "Name" varchar(100) NOT NULL
);

CREATE INDEX "IX_Categories_Name" ON "Categories" ("Name");

-- Create TodoItems table
CREATE TABLE "TodoItems" (
    "Id" SERIAL PRIMARY KEY,
    "Title" varchar(200) NOT NULL,
    "IsCompleted" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp NOT NULL,
    "DueTime" timestamp NULL,
    "CategoryId" integer NOT NULL,
    CONSTRAINT "FK_TodoItems_Categories_CategoryId"
        FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id")
        ON DELETE RESTRICT
);

CREATE INDEX "IX_TodoItems_CategoryId" ON "TodoItems" ("CategoryId");
CREATE INDEX "IX_TodoItems_CreatedAt" ON "TodoItems" ("CreatedAt");
CREATE INDEX "IX_TodoItems_IsCompleted" ON "TodoItems" ("IsCompleted");
```

**Educational Insight:**

- Notice `SERIAL` for auto-increment (PostgreSQL equivalent of MySQL's `AUTO_INCREMENT`)
- Foreign key constraint names follow convention: `FK_{dependent}_{principal}_{FK}`
- Indexes automatically named: `IX_{table}_{column}`

### Step 4: Apply Migration

**In Development (Automatic):**
Your `Program.cs` already has auto-migration enabled:

```csharp
// Auto-apply migrations in development
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
```

Just run the app and migrations apply automatically!

**Manual Application:**

```bash
dotnet ef database update
```

**Compare with Laravel:**

- Laravel: `php artisan migrate`
- .NET: `dotnet ef database update` (or auto-migrate on startup)

---

## Phase 2: Controllers with Direct DbContext (Educational Phase)

### Step 5: Create TodoItemsController with DbContext

**Why start with DbContext directly?**

- **Educational**: See how EF Core works under the hood
- Understand LINQ queries and how they translate to SQL
- Learn eager loading, lazy loading, and N+1 query problems
- **Then** refactor to repository pattern (similar to Laravel's journey from Query Builder to Eloquent to Repository)

**File:** `TodoApi/Controllers/TodoItemsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")] // Results in: /api/TodoItems
public class TodoItemsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TodoItemsController> _logger;

    public TodoItemsController(AppDbContext context, ILogger<TodoItemsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/TodoItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
        // Direct DbContext query - EDUCATIONAL STEP
        // This demonstrates EF Core query patterns

        // ⚠️ PROBLEM: This causes N+1 queries!
        // var todos = await _context.TodoItems.ToListAsync();
        // Each todo.Category access would trigger a separate query

        // ✅ SOLUTION: Use Include() for eager loading
        var todos = await _context.TodoItems
            .Include(t => t.Category)  // JOIN with Categories table
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} todo items", todos.Count);

        return Ok(todos);
    }

    // GET: api/TodoItems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
    {
        var todo = await _context.TodoItems
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (todo == null)
        {
            _logger.LogWarning("Todo item {Id} not found", id);
            return NotFound(new { message = $"Todo item with ID {id} not found" });
        }

        return Ok(todo);
    }

    // GET: api/TodoItems/category/2
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodosByCategory(int categoryId)
    {
        // Check if category exists
        var categoryExists = await _context.Set<Category>()
            .AnyAsync(c => c.Id == categoryId);

        if (!categoryExists)
        {
            return NotFound(new { message = $"Category with ID {categoryId} not found" });
        }

        var todos = await _context.TodoItems
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(todos);
    }
}
```

**Key Learning Points:**

1. **Dependency Injection**: Constructor receives `AppDbContext` and `ILogger`

   - Registered in `Program.cs` as scoped services
   - Similar to Laravel's constructor injection

2. **`[ApiController]` Attribute**: Automatic behaviors

   - Automatic model validation (400 Bad Request on invalid model)
   - Automatic inference of `[FromBody]`, `[FromRoute]`, `[FromQuery]`
   - Problem details for error responses (RFC 7807)

3. **Route Tokens**: `[Route("api/[controller]")]`

   - `[controller]` replaced with class name minus "Controller"
   - `TodoItemsController` → `/api/TodoItems`
   - Similar to Laravel's resource routing

4. **Async/Await Pattern**: All database operations are async

   - `async Task<ActionResult<T>>` return type
   - `await` keyword for async operations
   - Better scalability than synchronous code
   - Similar to Laravel's async database operations (though less common in PHP)

5. **EF Core Query Methods**:

   - `ToListAsync()`: Execute query and return list (like Laravel's `get()`)
   - `FirstOrDefaultAsync()`: Get first or null (like Laravel's `first()`)
   - `AnyAsync()`: Check existence (like Laravel's `exists()`)
   - `Include()`: Eager load relationships (like Laravel's `with()`)
   - `Where()`: Filter (like Laravel's `where()`)

6. **N+1 Query Problem**:

   ```csharp
   // ❌ BAD: Causes N+1 queries
   var todos = await _context.TodoItems.ToListAsync();
   // Later accessing todo.Category triggers individual queries

   // ✅ GOOD: One query with JOIN
   var todos = await _context.TodoItems.Include(t => t.Category).ToListAsync();
   ```

7. **ActionResult<T> Return Type**:
   - Can return `T` directly or `IActionResult` (status codes)
   - `Ok(data)` → 200 with JSON body
   - `NotFound()` → 404
   - Similar to Laravel's response helpers

---

## Phase 3: Repository Pattern Implementation

### Step 6: Create Repository Classes

**Why use Repository Pattern?**

- **Abstraction**: Controllers don't know about EF Core details
- **Testability**: Easy to mock repositories in unit tests
- **Reusability**: Complex queries defined once, used everywhere
- **Consistency**: Standard interface across all entities
- Similar to Laravel's Repository pattern (not built-in, but common)

**File:** `TodoApi/Models/TodoRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Models;

public interface ITodoRepository : IBaseRepository<TodoItem>
{
    Task<TodoItem?> GetTodoWithCategoryAsync(int id);
    Task<IEnumerable<TodoItem>> GetTodosByCategoryAsync(int categoryId);
    Task<IEnumerable<TodoItem>> GetCompletedTodosAsync();
    Task<IEnumerable<TodoItem>> GetPendingTodosAsync();
    Task<IEnumerable<TodoItem>> GetOverdueTodosAsync();
}

public class TodoRepository : BaseRepository<TodoItem>, ITodoRepository
{
    public TodoRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<TodoItem?> GetTodoWithCategoryAsync(int id)
    {
        // Domain-specific query encapsulated in repository
        return await DbSet
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TodoItem>> GetTodosByCategoryAsync(int categoryId)
    {
        return await DbSet
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetCompletedTodosAsync()
    {
        return await DbSet
            .Include(t => t.Category)
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetPendingTodosAsync()
    {
        return await DbSet
            .Include(t => t.Category)
            .Where(t => !t.IsCompleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetOverdueTodosAsync()
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(t => t.Category)
            .Where(t => !t.IsCompleted && t.DueTime.HasValue && t.DueTime.Value < now)
            .OrderBy(t => t.DueTime)
            .ToListAsync();
    }
}
```

**Key Learning Points:**

1. **Interface Segregation**: Each repository has its own interface

   - Inherits from `IBaseRepository<T>` (common operations)
   - Adds entity-specific methods
   - Similar to Laravel's Repository interfaces

2. **Inheritance Chain**:

   ```
   IBaseRepository<TodoItem>
        ↓
   ITodoRepository
        ↓
   BaseRepository<TodoItem>
        ↓
   TodoRepository
   ```

3. **Protected Members**: `BaseRepository` provides:

   - `Context` (AppDbContext)
   - `DbSet<T>` (direct access to entity set)
   - Common methods like `GetAllAsync()`

4. **Domain Logic**: Repositories encapsulate:
   - Complex queries (`GetOverdueTodosAsync`)
   - Business rules (filtering logic)
   - Eager loading strategy (which relationships to include)
   - Similar to Laravel's query scopes but more powerful

### Step 7: Register Repositories in Dependency Injection

**File:** `TodoApi/Program.cs` (add after existing repository registration)

```csharp
// Repository registration
builder.Services.AddScoped<IBaseRepository<WeatherForecast>, WeatherRepository>();

// Add these new registrations:
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
```

**Key Learning:**

- **Scoped lifetime**: New instance per HTTP request
- Matches `DbContext` lifetime (also scoped)
- Interface → Implementation mapping
- Similar to Laravel's service container binding: `$this->app->bind()`

### Step 8: Refactor Controller to Use Repository

**File:** `TodoApi/Controllers/TodoItemsController.cs` (refactored)

```csharp
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly ITodoRepository _todoRepository;
    private readonly ILogger<TodoItemsController> _logger;

    // Constructor injection: Now using repositories instead of DbContext
    public TodoItemsController(
        ITodoRepository todoRepository,
        ILogger<TodoItemsController> logger)
    {
        _todoRepository = todoRepository;
        _logger = logger;
    }

    // GET: api/todos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
        // Much cleaner! No EF Core details in controller
        var todos = await _todoRepository.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} todo items", todos.Count());
        return Ok(todos);
    }

    // GET: api/TodoItems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
    {
        // Repository method handles eager loading
        var todo = await _todoRepository.GetTodoWithCategoryAsync(id);

        if (todo == null)
        {
            _logger.LogWarning("Todo item {Id} not found", id);
            return NotFound(new { message = $"Todo item with ID {id} not found" });
        }

        return Ok(todo);
    }

    // GET: api/TodoItems/category/2
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodosByCategory(int categoryId)
    {
        // TODO: implement a method in the repository to check category existence if needed


        var todos = await _todoRepository.GetTodosByCategoryAsync(categoryId);
        return Ok(todos);
    }

    // GET: api/TodoItems/completed
    [HttpGet("completed")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetCompletedTodos()
    {
        var todos = await _todoRepository.GetCompletedTodosAsync();
        return Ok(todos);
    }

    // GET: api/TodoItems/pending
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetPendingTodos()
    {
        var todos = await _todoRepository.GetPendingTodosAsync();
        return Ok(todos);
    }

    // GET: api/TodoItems/overdue
    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetOverdueTodos()
    {
        var todos = await _todoRepository.GetOverdueTodosAsync();
        return Ok(todos);
    }
}
```

**Comparison: Before vs After Repository Pattern**

**Before (Direct DbContext):**

```csharp
var todos = await _context.TodoItems
    .Include(t => t.Category)
    .Where(t => !t.IsCompleted)
    .OrderByDescending(t => t.CreatedAt)
    .ToListAsync();
```

**After (Repository):**

```csharp
var todos = await _todoRepository.GetPendingTodosAsync();
```

**Benefits:**

- Controller is cleaner and focuses on HTTP concerns
- Query logic is reusable and testable
- Easy to mock in unit tests
- Changes to query strategy don't affect controller

---

## Phase 4: DTOs and Validation with FluentValidation

**Project Organization:**

This phase introduces a clear separation between input and output data:

- **`Requests/`** folder: Input DTOs and their validators (similar to Laravel Form Requests)

  - Request DTOs define what clients send to the API
  - Validators live alongside their requests for easy maintenance
  - Example: `CreateTodoItemRequest.cs` + `CreateTodoItemRequestValidator.cs`

- **`Controllers/TodoItemsResource.cs`**: Output DTOs (similar to Laravel API Resources)
  - Response DTOs define what the API returns to clients
  - Already exists with `TodoItemDto` and `CategoryDto`
  - Uses static methods like `TodoItemDto.Make()` for transformation

This structure provides:

- Clear separation of concerns (input vs output)
- Co-location of related code (request + validator)
- Similar to Laravel's Form Requests and API Resources pattern

### Step 9: Install FluentValidation

**Command:**

```bash
cd TodoApi
dotnet add package FluentValidation.AspNetCore
```

**Why FluentValidation?**

- **Similar to Laravel Form Requests**: Separate validation logic
- More powerful than Data Annotations
- Better testability
- Cleaner models (no validation attributes cluttering POCOs)

### Step 10: Create Request and Response DTOs

**Why separate Request and Response DTOs?**

- **Separate API contract from database model**: API can change without affecting DB
- **Security**: Control exactly what clients can send/receive
- **Validation**: Validate input without polluting domain models
- **Clarity**: Request DTOs (input) vs Response DTOs (output)
- Similar to Laravel's Form Requests and API Resources

**Project Structure:**

- `Requests/` folder: Input DTOs with their validators (similar to Laravel Form Requests)
- `Controllers/TodoItemsResource.cs`: Output DTOs (similar to Laravel API Resources)

**File:** `TodoApi/Requests/CreateTodoItemRequest.cs`

```csharp
namespace TodoApi.Requests;

/// <summary>
/// Request DTO for creating a new todo item.
/// Similar to Laravel Form Request.
/// </summary>
public class CreateTodoItemRequest
{
    public string Title { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateTime? DueTime { get; set; }
}
```

**Note:** Response DTOs (`TodoItemDto`, `CategoryDto`) already exist in `Controllers/TodoItemsResource.cs` and are used for API responses.

### Step 11: Create FluentValidation Validators

```csharp
namespace TodoApi.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TodoCount { get; set; }
}
```

_Note: These DTOs are shown for reference. For the TODO creation focus, only `CreateTodoItemRequest` is needed now._

### Step 11: Create FluentValidation Validators

**Important:** Validators are co-located with their request DTOs in the `Requests/` folder. This keeps validation logic close to the data it validates, similar to Laravel Form Requests.

**File:** `TodoApi/Requests/CreateTodoItemRequestValidator.cs`

```csharp
using FluentValidation;
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
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0")
            .MustAsync(CategoryExistsAsync).WithMessage("Category does not exist");

        // DueTime validation (optional but must be future date if provided)
        RuleFor(x => x.DueTime)
            .Must(BeAFutureDate).WithMessage("Due time must be in the future")
            .When(x => x.DueTime.HasValue);
    }

    /// <summary>
    /// Async validation: Check if category exists in database.
    /// Similar to Laravel's 'exists:categories,id' rule but async.
    /// </summary>
    private async Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        return await _categoryRepository.ExistsAsync(categoryId);
    }

    /// <summary>
    /// Custom validation: Check if due time is in the future.
    /// </summary>
    private bool BeAFutureDate(DateTime? dueTime)
    {
        return !dueTime.HasValue || dueTime.Value > DateTime.UtcNow;
    }
}
```

**Key Learning Points:**

1. **Co-location Pattern**: Validators live with their request DTOs in `Requests/` folder

   - Keeps related code together (similar to Laravel Form Requests)
   - Easy to find validation rules for a specific request
   - Clear separation from response DTOs

2. **Validator Registration**: Validators need dependency injection

   - Can inject repositories for database validation
   - Similar to Laravel Form Requests with database rules
   - Auto-discovered by `AddValidatorsFromAssemblyContaining<Program>()`

3. **Validation Rules**:

   - `NotEmpty()`: Required field (like Laravel's `required`)
   - `MaximumLength()`: Max length (like Laravel's `max`)
   - `Must()`: Custom sync validation
   - `MustAsync()`: Custom async validation (database checks)
   - `When()`: Conditional validation (like Laravel's `sometimes`)

4. **Async Validation**: Can query database during validation
   - Check if CategoryId exists using `ExistsAsync()`
   - Check for unique constraints
   - More powerful than Data Annotations

### Step 12: Register FluentValidation in Program.cs

**File:** `TodoApi/Program.cs`

```csharp
using FluentValidation;
using FluentValidation.AspNetCore;

// Add after AddControllers()
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

**What This Does:**

- `AddFluentValidationAutoValidation()`: Automatic validation before controller actions
- `AddValidatorsFromAssemblyContaining<Program>()`: Auto-discover all validators
- Validators are registered as scoped services (can use DI)

---

## Phase 5: Full CRUD Operations

### Step 13: Expand BaseRepository with Create/Update/Delete

**First, update IBaseRepository interface:**

**File:** `TodoApi/Models/IBaseRepository.cs`

```csharp
namespace TodoApi.Models;

public interface IBaseRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

**Then update BaseRepository implementation:**

**File:** `TodoApi/Models/BaseRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Models;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public BaseRepository(AppDbContext context)
    {
        Context = context;
        DbSet = Context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        DbSet.Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }
}
```

### Step 14: Create Complete TodoItemsController with Full CRUD

**File:** `TodoApi/Controllers/TodoItemsController.cs` (complete version)

```csharp
using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly ITodoRepository _todoRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<TodoItemsController> _logger;

    public TodoItemsController(
        ITodoRepository todoRepository,
        ICategoryRepository categoryRepository,
        ILogger<TodoItemsController> logger)
    {
        _todoRepository = todoRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    // GET: api/TodoItems
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TodoItemResponseDto>>> GetTodoItems()
    {
        var todos = await _todoRepository.GetAllAsync();
        var response = todos.Select(MapToResponseDto);
        return Ok(response);
    }

    // GET: api/TodoItems/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItemResponseDto>> GetTodoItem(int id)
    {
        var todo = await _todoRepository.GetTodoWithCategoryAsync(id);

        if (todo == null)
        {
            return NotFound(new { message = $"Todo item with ID {id} not found" });
        }

        return Ok(MapToResponseDto(todo));
    }

    // POST: api/TodoItems
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoItemResponseDto>> CreateTodoItem(CreateTodoItemRequest request)
    {
        // FluentValidation automatically validates before this point
        // If validation fails, returns 400 with validation errors

        var todoItem = new TodoItem
        {
            Title = request.Title,
            CategoryId = request.CategoryId,
            DueTime = request.DueTime,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        var created = await _todoRepository.AddAsync(todoItem);

        // Load category for response
        var todoWithCategory = await _todoRepository.GetTodoWithCategoryAsync(created.Id);

        _logger.LogInformation("Created todo item {Id}: {Title}", created.Id, created.Title);

        // Return 201 Created with Location header
        return CreatedAtAction(
            nameof(GetTodoItem),
            new { id = created.Id },
            MapToResponseDto(todoWithCategory!)
        );
    }

    // PUT: api/TodoItems/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoItemResponseDto>> UpdateTodoItem(
        int id,
        UpdateTodoItemDto dto)
    {
        var existingTodo = await _todoRepository.GetByIdAsync(id);

        if (existingTodo == null)
        {
            return NotFound(new { message = $"Todo item with ID {id} not found" });
        }

        // Update only provided fields (partial update)
        if (!string.IsNullOrEmpty(dto.Title))
            existingTodo.Title = dto.Title;

        if (dto.IsCompleted.HasValue)
            existingTodo.IsCompleted = dto.IsCompleted.Value;

        if (dto.CategoryId.HasValue)
            existingTodo.CategoryId = dto.CategoryId.Value;

        if (dto.DueTime.HasValue)
            existingTodo.DueTime = dto.DueTime;

        var updated = await _todoRepository.UpdateAsync(existingTodo);
        var todoWithCategory = await _todoRepository.GetTodoWithCategoryAsync(updated.Id);

        _logger.LogInformation("Updated todo item {Id}", id);

        return Ok(MapToResponseDto(todoWithCategory!));
    }

    // DELETE: api/TodoItems/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTodoItem(int id)
    {
        var deleted = await _todoRepository.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"Todo item with ID {id} not found" });
        }

        _logger.LogInformation("Deleted todo item {Id}", id);

        return NoContent(); // 204 No Content (successful deletion)
    }

    // Additional query endpoints...

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<TodoItemResponseDto>>> GetTodosByCategory(int categoryId)
    {
        var todos = await _todoRepository.GetTodosByCategoryAsync(categoryId);
        return Ok(todos.Select(MapToResponseDto));
    }

    [HttpGet("completed")]
    public async Task<ActionResult<IEnumerable<TodoItemResponseDto>>> GetCompletedTodos()
    {
        var todos = await _todoRepository.GetCompletedTodosAsync();
        return Ok(todos.Select(MapToResponseDto));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<TodoItemResponseDto>>> GetPendingTodos()
    {
        var todos = await _todoRepository.GetPendingTodosAsync();
        return Ok(todos.Select(MapToResponseDto));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<TodoItemResponseDto>>> GetOverdueTodos()
    {
        var todos = await _todoRepository.GetOverdueTodosAsync();
        return Ok(todos.Select(MapToResponseDto));
    }

    // Helper method to map entity to DTO
    private TodoItemResponseDto MapToResponseDto(TodoItem todo)
    {
        return new TodoItemResponseDto
        {
            Id = todo.Id,
            Title = todo.Title,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            DueTime = todo.DueTime,
            Category = todo.Category != null ? new CategoryResponseDto
            {
                Id = todo.Category.Id,
                Name = todo.Category.Name,
                TodoCount = 0 // Not loaded in this context
            } : null
        };
    }
}
```

**Key Learning Points:**

1. **HTTP Status Codes** (REST conventions):

   - `200 OK`: Successful GET, PUT
   - `201 Created`: Successful POST (with Location header)
   - `204 No Content`: Successful DELETE
   - `400 Bad Request`: Validation errors
   - `404 Not Found`: Resource doesn't exist

2. **ProducesResponseType Attributes**: API documentation

   - Helps Swagger/OpenAPI generate correct documentation
   - Similar to Laravel's API resource responses

3. **CreatedAtAction**: Returns 201 with Location header

   ```csharp
   CreatedAtAction(nameof(GetTodoItem), new { id = 5 }, todoDto)
   ```

   Results in: `Location: /api/TodoItems/5`

4. **Partial Updates**: PUT accepts partial data

   - Only update fields that are provided
   - Similar to Laravel's `$request->filled()`

5. **Mapping**: Entity → DTO conversion
   - Could use AutoMapper library for complex mappings
   - Manual mapping shown here for educational clarity

### Step 15: Create CategoriesController

**File:** `TodoApi/Controllers/CategoriesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryRepository categoryRepository,
        ILogger<CategoriesController> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    // GET: api/Categories
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
    {
        var categories = await _categoryRepository.GetCategoriesWithTodoCountAsync();
        var response = categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            TodoCount = c.TodoItems.Count
        });
        return Ok(response);
    }

    // GET: api/Categories/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponseDto>> GetCategory(int id)
    {
        var category = await _categoryRepository.GetCategoryWithTodosAsync(id);

        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        return Ok(new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            TodoCount = category.TodoItems.Count
        });
    }

    // POST: api/Categories
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name
        };

        var created = await _categoryRepository.AddAsync(category);

        _logger.LogInformation("Created category {Id}: {Name}", created.Id, created.Name);

        var response = new CategoryResponseDto
        {
            Id = created.Id,
            Name = created.Name,
            TodoCount = 0
        };

        return CreatedAtAction(nameof(GetCategory), new { id = created.Id }, response);
    }

    // PUT: api/Categories/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(
        int id,
        UpdateCategoryDto dto)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);

        if (existingCategory == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        existingCategory.Name = dto.Name;
        var updated = await _categoryRepository.UpdateAsync(existingCategory);

        _logger.LogInformation("Updated category {Id}", id);

        return Ok(new CategoryResponseDto
        {
            Id = updated.Id,
            Name = updated.Name,
            TodoCount = 0
        });
    }

    // DELETE: api/Categories/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        // Check if category has todos
        var hasTodos = await _categoryRepository.HasTodosAsync(id);

        if (hasTodos)
        {
            return BadRequest(new
            {
                message = "Cannot delete category with existing todo items. Please reassign or delete todos first."
            });
        }

        var deleted = await _categoryRepository.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        _logger.LogInformation("Deleted category {Id}", id);

        return NoContent();
    }
}
```

**Key Learning: Business Logic in Controller**

- Delete validation (prevent deleting category with todos)
- Could also be in repository or a separate service layer
- Trade-off: Controller simplicity vs. business logic centralization

---

## Phase 6: Testing and Documentation

### Testing Recommendations

**Unit Tests** (test repositories and validators):

```csharp
// Example: Test TodoRepository
public class TodoRepositoryTests
{
    [Fact]
    public async Task GetTodoWithCategoryAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange: In-memory database
        // Act: Call repository method
        // Assert: Verify result
    }
}
```

**Integration Tests** (test full API endpoints):

```csharp
// Example: Test TodoItemsController
public class TodoItemsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetTodoItems_ReturnsOkResult_WithTodos()
    {
        // Arrange: Test client
        // Act: HTTP GET request
        // Assert: Status 200, correct data
    }
}
```

### API Documentation with Swagger

Your project already has Swagger enabled. Access it at:

```
https://localhost:{port}/swagger
```

**Enhance with XML comments:**

```csharp
/// <summary>
/// Creates a new todo item
/// </summary>
/// <param name="dto">Todo item details</param>
/// <returns>The created todo item</returns>
/// <response code="201">Todo item created successfully</response>
/// <response code="400">Invalid input data</response>
[HttpPost]
[ProducesResponseType(typeof(TodoItemResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<TodoItemResponseDto>> CreateTodoItem(CreateTodoItemDto dto)
```

---

## Summary: Laravel vs ASP.NET Comparison

| Concept                  | Laravel                    | ASP.NET Core                                |
| ------------------------ | -------------------------- | ------------------------------------------- |
| **Models**               | Eloquent Model             | POCO + EF Core Entity                       |
| **Relationships**        | `belongsTo()`, `hasMany()` | Navigation Properties + Fluent API          |
| **Migrations**           | Write migrations first     | Generate from models                        |
| **Validation**           | Form Requests              | DTOs + FluentValidation                     |
| **Repositories**         | Manual implementation      | Manual (BaseRepository pattern)             |
| **Dependency Injection** | Service Container          | Built-in DI Container                       |
| **Controllers**          | Extend `Controller`        | Extend `ControllerBase` + `[ApiController]` |
| **Routing**              | `routes/api.php`           | Attribute routing `[Route]`                 |
| **Query Builder**        | Eloquent methods           | LINQ methods                                |
| **Eager Loading**        | `with('relation')`         | `Include(x => x.Relation)`                  |

---

## Next Steps for Learning

1. **Add Authentication**: JWT Bearer tokens, similar to Laravel Sanctum
2. **Add Pagination**: `PagedList` pattern for large datasets
3. **Add Filtering/Sorting**: Query parameters with LINQ
4. **Error Handling**: Global exception middleware
5. **Logging**: Structured logging with Serilog
6. **Caching**: Response caching, distributed caching
7. **API Versioning**: Multiple API versions
8. **Rate Limiting**: Protect your API from abuse

---

## Commands Cheat Sheet

```bash
# Run the application
dotnet run

# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName

# Generate SQL script
dotnet ef migrations script

# Remove last migration (if not applied)
dotnet ef migrations remove

# Add package
dotnet add package PackageName

# Restore packages
dotnet restore

# Run tests
dotnet test
```

---

**Happy Coding! 🚀**
