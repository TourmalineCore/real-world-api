using Application.Queries.Contracts;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries
{
    public class GetAllToDosQuery : IGetAllToDosQuery
    {
        private readonly AppDbContext _context;

        public GetAllToDosQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ToDo>> GetAllAsync(long tenantId)
        {
            var toDoList = await _context
                .ToDos
                .Where(x => x.TenantId == tenantId)
                .ToListAsync();
            return toDoList;
        }
    }
}