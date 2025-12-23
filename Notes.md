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

## Request

- Laravel => $request + Request class + Form Requests
- ASP.NET .NET => HttpRequest + Model Binding + Data Annotations

## Package Management

- Laravel => Composer + composer.json + composer.phar
- ASP.NET .NET => NuGet + .csproj file

### Project Layers

- Controllers => Controllers
- Models => Models
