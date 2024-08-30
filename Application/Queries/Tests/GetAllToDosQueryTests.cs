using Xunit;
using Application;
using Application.Queries;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
public class GetAllToDosQueryTests
{
    private readonly AppDbContext _context;
    private readonly GetAllToDosQuery _query;

    public GetAllToDosQueryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "GetAllToDosQueryToDoDatabase")
            .Options;

        _context = new AppDbContext(options);
        _query = new GetAllToDosQuery(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllToDos()
    {
        var tenantId = 3;
        var toDo1 = new ToDo { Id = 1, Name = "Test ToDo 01", TenantId = 3L };
        var toDo2 = new ToDo { Id = 2, Name = "Test ToDo 02", TenantId = 3L };
        var toDo3 = new ToDo { Id = 3, Name = "Test ToDo 03", TenantId = 3L };

        _context.ToDos.AddRange(toDo1, toDo2, toDo3);
        await _context.SaveChangesAsync();

        var result = await _query.GetAllAsync(tenantId);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.Id == 1 && t.Name == "Test ToDo 01");
        Assert.Contains(result, t => t.Id == 2 && t.Name == "Test ToDo 02");
        Assert.Contains(result, t => t.Id == 3 && t.Name == "Test ToDo 03");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllToDosFromSpecificTenant()
    {
        var tenantId = 1;
        var toDo1 = new ToDo { Id = 4, Name = "Test ToDo 4", TenantId = 1L };
        var toDo2 = new ToDo { Id = 5, Name = "Test ToDo 5", TenantId = 2L };
        var toDo3 = new ToDo { Id = 6, Name = "Test ToDo 6", TenantId = 1L };

        _context.ToDos.AddRange(toDo1, toDo2, toDo3);
        await _context.SaveChangesAsync();

        var result = await _query.GetAllAsync(tenantId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Id == 4 && t.Name == "Test ToDo 4");
        Assert.Contains(result, t => t.Id == 6 && t.Name == "Test ToDo 6");
    }
}