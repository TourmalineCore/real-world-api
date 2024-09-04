using System.Security.Claims;
using Api.Responses;
using Application.Commands.Contracts;
using Application.Queries.Contracts;
using Application.Requests;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ToDo = Core.Entities.ToDo;

namespace Api.Controllers
{
    public class ToDoControllerTests
    {
        private readonly Mock<IGetAllToDosQuery> _getAllToDosQueryMock;
        private readonly Mock<ICreateToDoCommand> _createToDoCommandMock;
        private readonly Mock<IDeleteToDoCommand> _deleteToDoCommandMock;
        private readonly Mock<ISoftDeleteToDoCommand> _softDeleteToDoCommandMock;
        private readonly Mock<ToDoService> _toDoServiceMock;
        private readonly ToDoController _controller;

        public ToDoControllerTests()
        {
            _getAllToDosQueryMock = new Mock<IGetAllToDosQuery>();
            _createToDoCommandMock = new Mock<ICreateToDoCommand>();
            _deleteToDoCommandMock = new Mock<IDeleteToDoCommand>();
            _softDeleteToDoCommandMock = new Mock<ISoftDeleteToDoCommand>();
            _toDoServiceMock = new Mock<ToDoService>(new Mock<IDeleteToDoCommand>().Object);

            _controller = new ToDoController(
                    _toDoServiceMock.Object,
                    _getAllToDosQueryMock.Object,
                    _createToDoCommandMock.Object,
                    _deleteToDoCommandMock.Object,
                    _softDeleteToDoCommandMock.Object
                );

            var claims = new List<Claim>
            {
                new Claim("tenantId", "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(x => x.User).Returns(user);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object
            };
        }

        [Fact]
        public async Task GetAllToDosAsync_ShouldReturnToDosResponse()
        {
            var tenantId = 1L;

            var toDoItems = new List<ToDo>
            {
                new()
                {
                    Id = 1,
                    Name = "Test ToDo 1",
                    TenantId = 1L,
                },
                new()
                {
                    Id = 2,
                    Name = "Test ToDo 2",
                    TenantId = 1L,
                },
            };

            _getAllToDosQueryMock
                .Setup(query => query.GetAllAsync(tenantId))
                .ReturnsAsync(toDoItems);

            var result = await _controller.GetAllToDosAsync();

            Assert.IsType<ToDosResponse>(result);
            Assert.Equal(2, result.ToDos.Count);
            Assert.Equal("Test ToDo 1", result.ToDos[0].Name);
            _getAllToDosQueryMock.Verify(query => query.GetAllAsync(tenantId), Times.Once);
        }

        [Fact]
        public async Task AddToDoAsync_ShouldReturnCreatedToDoId()
        {
            var tenantId = 1L;

            var request = new AddToDoRequest
            {
                Name = "New ToDo",
            };

            _createToDoCommandMock
                .Setup(command => command.CreateAsync(request, tenantId))
                .ReturnsAsync(1);

            var result = await _controller.AddToDoAsync(request);

            Assert.Equal(1, result);
            _createToDoCommandMock.Verify(command => command.CreateAsync(request, tenantId), Times.Once);
        }

        [Fact]
        public async Task CompleteToDo_ShouldCallCompleteToDoAsync()
        {
            var tenantId = 1L;

            var request = new CompleteToDoRequest
            {
                ToDoIds = new List<long>
                {
                    1,
                    2,
                },
            };

            await _controller.CompleteToDo(request);

            _toDoServiceMock.Verify(service => service.CompleteToDoAsync(request.ToDoIds, tenantId), Times.Once);
        }

        [Fact]
        public async Task DeleteToDo_ShouldCallDeleteAsync()
        {
            var toDoId = 1L;
            var tenantId = 1L;
            await _controller.DeleteToDo(toDoId);

            _deleteToDoCommandMock.Verify(command => command.DeleteAsync(toDoId, tenantId), Times.Once);
        }
    }
}