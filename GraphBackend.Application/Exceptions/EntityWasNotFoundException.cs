using GraphBackend.Domain.Exceptions;

namespace GraphBackend.Application.Exceptions;

public class EntityWasNotFoundException : NotFound404Exception
{
    public EntityWasNotFoundException(string? message = "Сущность не найдена") : base(message)
    {
    }
    
    public EntityWasNotFoundException(string entityName, int id) : base(
        $"Не удалось найти '{entityName} в базе с ID = {id}'")
    {
    }
    
    public EntityWasNotFoundException(string entityName, Enum id) : base(
        $"Не удалось найти '{entityName} в базе с ID = {Convert.ToInt32(id)}'")
    {
    }
}