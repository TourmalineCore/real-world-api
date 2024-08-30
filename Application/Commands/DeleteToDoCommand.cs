using Application.Commands.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands
{
    public class DeleteToDoCommand : IDeleteToDoCommand
    {
        private readonly AppDbContext _context;

        public DeleteToDoCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAsync(long id, long tenantId)
        {
            var toDo = await _context
                .ToDos
                .Where(x => x.Id == id && x.TenantId == tenantId)
                .SingleAsync();
            _context.ToDos.Remove(toDo);
            await _context.SaveChangesAsync();
        }
    }
}
