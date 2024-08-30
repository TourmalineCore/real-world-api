using Xunit;
using Application;
using Application.Queries;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

public class GetToDoByIdQueryTests
{
    private readonly AppDbContext _context;
    private readonly GetToDoByIdQuery _query;

    public GetToDoByIdQueryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "GetToDoByIdQueryToDoDatabase")
            .Options;

        _context = new AppDbContext(options);
        _query = new GetToDoByIdQuery(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnToDo_WhenToDoExists()
    {
        var tenantId = 1;
        var toDo = new ToDo { Id = 1, Name = "Test ToDo", TenantId = 1};
        _context.ToDos.Add(toDo);
        await _context.SaveChangesAsync();

        var result = await _query.GetByIdAsync(toDo.Id, tenantId);

        Assert.NotNull(result);
        Assert.Equal(toDo.Id, result.Id);
        Assert.Equal(toDo.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenToDoDoesNotExist()
    {
        var tenantId = 1;
        var nonExistentId = 999;

        var result = await _query.GetByIdAsync(nonExistentId, tenantId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldntReturnToDo_WhenToDoExistsInAnotherTenant()
    {
        var tenantId = 1;
        var toDo = new ToDo { Id = 2, Name = "Test ToDo", TenantId = 1L };
        var toDo1 = new ToDo { Id = 3, Name = "Test ToDo", TenantId = 2L };
        var toDo2 = new ToDo { Id = 4, Name = "Test ToDo", TenantId = 1L };
        _context.ToDos.Add(toDo);
        _context.ToDos.Add(toDo1);
        _context.ToDos.Add(toDo2);
        await _context.SaveChangesAsync();

        var result = await _query.GetByIdAsync(toDo1.Id, tenantId);

        Assert.Null(result);
    }
}