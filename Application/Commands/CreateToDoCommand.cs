using Application.Commands.Contracts;
using Application.Requests;
using Core.Entities;
using NodaTime;

namespace Application.Commands
{
    public class CreateToDoCommand : ICreateToDoCommand
    {
        private readonly IClock _clock;
        private readonly AppDbContext _context;

        public CreateToDoCommand(AppDbContext context, IClock clock)
        {
            _context = context;
            _clock = clock;
        }

        public async Task<long> CreateAsync(AddToDoRequest addToDoRequest, long tenantId)
        {
            var toDo = new ToDo(addToDoRequest.Name, _clock, tenantId);
            await _context.ToDos.AddAsync(toDo);
            await _context.SaveChangesAsync();
            return toDo.Id;
        }
    }
}