using Xunit;
using Application;
using Application.Commands;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

public class DeleteToDoCommandTests
{
    private readonly AppDbContext _context;
    private readonly DeleteToDoCommand _command;

    public DeleteToDoCommandTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "DeleteToDoCommandToDoDatabase")
            .Options;

        _context = new AppDbContext(options);
        _command = new DeleteToDoCommand(_context);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveToDoFromDbSet()
    {
        var tenantId = 1L;
        var toDo = new ToDo { Id = 1, Name = "Test ToDo", TenantId = 1L };
        _context.ToDos.Add(toDo);
        await _context.SaveChangesAsync();

        await _command.DeleteAsync(toDo.Id, tenantId);

        var deletedToDo = await _context.ToDos.FindAsync(toDo.Id);
        Assert.Null(deletedToDo);
    }

    [Fact]
    public async Task DeleteAsync_ShouldntRemoveToDoFromDbSet_IfItIsInAnotherTenant()
    {
        var tenantId = 2L;
        var toDo = new ToDo { Id = 2, Name = "Test ToDo", TenantId = 1L };
        _context.ToDos.Add(toDo);
        await _context.SaveChangesAsync();
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _command.DeleteAsync(toDo.Id, tenantId));
    }
}