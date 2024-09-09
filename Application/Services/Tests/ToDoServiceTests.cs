using Application.Commands.Contracts;
using Application.Services;
using Moq;
using Xunit;

public class ToDoServiceTests
{
    [Fact]
    public async Task CompleteToDoAsync_ShouldCallDeleteAsync_ForEachToDoId()
    {
        var tenantId = 1L;
        var deleteToDoCommandMock = new Mock<IDeleteToDoCommand>();
        var toDoService = new ToDoService(deleteToDoCommandMock.Object);

        var toDoIds = new List<long>
        {
            1,
            2,
            3,
        };

        await toDoService.CompleteToDoAsync(toDoIds, tenantId);

        foreach (var toDoId in toDoIds)
        {
            deleteToDoCommandMock.Verify(m => m.DeleteAsync(toDoId, tenantId), Times.Once);
        }
    }
}