using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Resources;

namespace TodoApi.Tests.IntegrationTests;

/// <summary>
/// Integration tests for TodoItems GET by ID endpoint.
/// These tests verify the entire request pipeline: routing → controller → repository → database → response.
/// Similar to Laravel's Feature Tests.
/// </summary>
public class GetTodoByIdTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetTodoByIdTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Test: GET /api/todos/{id} returns 200 OK with correct todo item.
    /// Verifies: Successful retrieval, correct data mapping, category relationship loaded.
    /// </summary>
    [Fact(DisplayName = "Get Todo by ID - Existing ID returns 200 OK with Todo item")]
    public async Task GetTodoById_ExistingId_ReturnsOkWithTodoItem()
    {
        // Arrange: Seed test data into in-memory database
        var (categoryId, todoId) = await SeedTestDataAsync();

        // Act: Send HTTP GET request
        var response = await _client.GetAsync($"/api/todos/{todoId}");

        // Assert: Check status code
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: Deserialize and verify response body
        var todoDto = await response.Content.ReadFromJsonAsync<TodoItemDto>();
        todoDto.Should().NotBeNull();
        todoDto!.Id.Should().Be(todoId);
        todoDto.Title.Should().Be("Test Todo Item");
        todoDto.IsCompleted.Should().BeFalse();
        todoDto.DueTime.Should().NotBeNull();

        // Assert: Category relationship is loaded
        todoDto.Category.Should().NotBeNull();
        todoDto.Category!.Id.Should().Be(categoryId);
        todoDto.Category.Name.Should().Be("Test Category");
    }

    /// <summary>
    /// Test: GET /api/todos/{id} returns 404 Not Found for non-existent ID.
    /// Verifies: Proper error handling and HTTP status codes.
    /// </summary>
    [Fact]
    public async Task GetTodoById_NonExistentId_ReturnsNotFound()
    {
        // Arrange: Use an ID that doesn't exist
        var nonExistentId = 99999;

        // Act: Send HTTP GET request
        var response = await _client.GetAsync($"/api/todos/{nonExistentId}");

        // Assert: Should return 404 Not Found
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Assert: Response should contain error message
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("not found");
    }

    /// <summary>
    /// Test: GET /api/todos/{id} with invalid ID format returns 400 Bad Request.
    /// Verifies: Model binding validation.
    /// </summary>
    [Theory]
    [InlineData("abc")]      // String instead of int
    [InlineData("12.5")]     // Decimal instead of int
    public async Task GetTodoById_InvalidIdFormat_ReturnsBadRequest(string invalidId)
    {
        // Act: Send HTTP GET request with invalid ID
        var response = await _client.GetAsync($"/api/todos/{invalidId}");

        // Assert: Should return 400 Bad Request (ASP.NET model binding validation)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Test: Verify that completed todo item has IsCompleted = true.
    /// Verifies: Correct handling of boolean properties.
    /// </summary>
    [Fact]
    public async Task GetTodoById_CompletedTodo_ReturnsIsCompletedTrue()
    {
        // Arrange: Seed completed todo
        var todoId = await SeedCompletedTodoAsync();

        // Act: Send HTTP GET request
        var response = await _client.GetAsync($"/api/todos/{todoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todoDto = await response.Content.ReadFromJsonAsync<TodoItemDto>();
        todoDto!.IsCompleted.Should().BeTrue();
    }

    /// <summary>
    /// Test: Todo without due time should return null for DueTime.
    /// Verifies: Proper handling of nullable DateTime properties.
    /// </summary>
    [Fact]
    public async Task GetTodoById_TodoWithoutDueTime_ReturnsDueTimeNull()
    {
        // Arrange: Seed todo without due time
        var todoId = await SeedTodoWithoutDueTimeAsync();

        // Act: Send HTTP GET request
        var response = await _client.GetAsync($"/api/todos/{todoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todoDto = await response.Content.ReadFromJsonAsync<TodoItemDto>();
        todoDto!.DueTime.Should().BeNull();
    }

    #region Helper Methods for Seeding Test Data

    /// <summary>
    /// Seeds a test category and todo item into the database.
    /// Returns the IDs for use in test assertions.
    /// Similar to Laravel's factory pattern.
    /// </summary>
    private async Task<(int CategoryId, int TodoId)> SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create category
        var category = new Category
        {
            Name = "Test Category"
        };
        context.Set<Category>().Add(category);
        await context.SaveChangesAsync();

        // Create todo item
        var todoItem = new TodoItem
        {
            Title = "Test Todo Item",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            DueTime = DateTime.UtcNow.AddDays(7),
            CategoryId = category.Id
        };
        context.Set<TodoItem>().Add(todoItem);
        await context.SaveChangesAsync();

        return (category.Id, todoItem.Id);
    }

    /// <summary>
    /// Seeds a completed todo item for testing.
    /// </summary>
    private async Task<int> SeedCompletedTodoAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create category
        var category = new Category { Name = "Completed Category" };
        context.Set<Category>().Add(category);
        await context.SaveChangesAsync();

        // Create completed todo
        var todoItem = new TodoItem
        {
            Title = "Completed Todo",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            CategoryId = category.Id
        };
        context.Set<TodoItem>().Add(todoItem);
        await context.SaveChangesAsync();

        return todoItem.Id;
    }

    /// <summary>
    /// Seeds a todo item without a due time.
    /// </summary>
    private async Task<int> SeedTodoWithoutDueTimeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create category
        var category = new Category { Name = "No Due Time Category" };
        context.Set<Category>().Add(category);
        await context.SaveChangesAsync();

        // Create todo without due time
        var todoItem = new TodoItem
        {
            Title = "Todo Without Due Time",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            DueTime = null, // Explicitly null
            CategoryId = category.Id
        };
        context.Set<TodoItem>().Add(todoItem);
        await context.SaveChangesAsync();

        return todoItem.Id;
    }

    #endregion
}
