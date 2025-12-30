using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using NSwag.AspNetCore;
using FluentValidation;
using FluentValidation.AspNetCore;

using TodoApi.Data;
using TodoApi.Repositories;
using TodoApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add DbContext - conditionally use PostgreSQL or SQLite based on environment
// This allows tests to override with SQLite without conflicts
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Register repositories for dependency injection
// Scoped lifetime: new instance per HTTP request (matches DbContext lifetime)
// Similar to Laravel's service container: $this->app->bind()
builder.Services.AddScoped<IBaseRepository<WeatherForecast>, TodoApi.Repositories.WeatherRepository>();
builder.Services.AddScoped<IBaseRepository<Category>, CategoryRepository>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddControllers();

// Add CORS policy to allow requests from the TodoUI frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTodoUI", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add FluentValidation
// - Auto-validates DTOs before controller actions
// - Auto-discovers validators from assembly
// - Validators use DI (can inject repositories for database validation)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "My API";
    options.Version = "v1";
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// Run migrations automatically in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //   app.MapOpenApi();

    app.UseOpenApi();

    app.UseSwaggerUi();    // /swagger
}

app.UseHttpsRedirection();

// Enable CORS - must be before UseAuthorization
app.UseCors("AllowTodoUI");

// app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for integration testing
// This allows WebApplicationFactory<Program> to reference the startup class
namespace TodoApi
{
    public partial class Program { }
}

