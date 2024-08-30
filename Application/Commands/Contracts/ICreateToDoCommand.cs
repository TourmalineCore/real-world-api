using Application.Requests;

namespace Application.Commands.Contracts;

public interface ICreateToDoCommand
{
    Task<long> CreateAsync(AddToDoRequest addToDoRequest, long tenantId);
}