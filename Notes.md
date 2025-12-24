# Learning C# and asp.net .NET

This repository contains my notes and sample code while learning C# and asp.net .NET.
as I've a background in PHP and Laravel framework, I try to find similarities and differences between them.

## Equivalences between Laravel and ASP.NET .NET

### CLI

- Artisan (`php artisan`) => .NET CLI (`dotnet`)

### ORM

- Laravel => Eloquent ORM
- ASP.NET .NET => Entity Framework Core (EF Core)

#### quick mental comparation

| Laravel (Eloquent) | C# (.NET)                     |
| ------------------ | ----------------------------- |
| Model              | Entity                        |
| $fillable          | Data Annotations / Fluent API |
| User::all()        | \_context.Users.ToList()      |
| Scopes             | LINQ methods                  |
| Relations          | Navigation Properties         |
| Migrations         | EF Core Migrations            |
| Soft Deletes       | Global Query Filters          |

## Migrations

- in Laravel changes are migration-first, the SoT is the migration files
- in ASP.NET .NET model-first the SoT is the model classes

> In ASP.NET Core, you evolve the database by evolving your models.
> Migrations are a generated artifact, not the main design tool.

### Typical Workflow (Recommended)

1. Change your model.

```csharp
// ✅ Clear model with explicit FK
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueTime { get; set; }
    public int CategoryId { get; set; }      // Explicit FK
    public Category? Category { get; set; }   // Navigation (nullable for optional)
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Collection navigation property
    public List<TodoItem> TodoItems { get; set; } = new();
}
```

### optional step if required - create Fluent API configuration

### Step 2: Generate migration

```bash
  dotnet ef migrations add AddTodoItemAndCategory
```

### Step 3: review sql migration code

```bash
dotnet ef migrations script
```

### Step 4: Apply migration

```bash
  dotnet ef database update
```

## Request

- Laravel => $request + Request class + Form Requests
- ASP.NET .NET => HttpRequest + Model Binding + Data Annotations

## Package Management

- Laravel => Composer + composer.json + composer.phar
- ASP.NET .NET => NuGet + .csproj file

### Project Layers

Laravel => Asp.NET

- FormRequests => DTO + Fluent Validation
- Services => Services
- Repositories => Repositories
- Controllers => Controllers
- Models => Models
- Resources => DTOs + AutoMapper (for scaling)
