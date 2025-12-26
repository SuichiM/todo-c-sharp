using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using NSwag.AspNetCore;

using TodoApi.Data;
using TodoApi.Repositories;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories for dependency injection
// Scoped lifetime: new instance per HTTP request (matches DbContext lifetime)
// Similar to Laravel's service container: $this->app->bind()
builder.Services.AddScoped<IBaseRepository<WeatherForecast>, TodoApi.Repositories.WeatherRepository>();
builder.Services.AddScoped<IBaseRepository<Category>, CategoryRepository>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddControllers();

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

// app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
