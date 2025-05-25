using GraphBackend.Domain.Exceptions;

namespace GraphBackend.Application.Exceptions;

public class RequiredEntityWasNotFoundException : NotFound404Exception
{
    public RequiredEntityWasNotFoundException(string entityName, int id) : base(
        $"Объект '{entityName}' по id {id} не был найден в базе")
    {
    }
}