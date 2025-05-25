using GraphBackend.Domain.Exceptions;

namespace GraphBackend.Application.Exceptions;

public class RequiredEntityIdWasNullException : BadRequest400Exception
{
    public RequiredEntityIdWasNullException(string entityName) : base(
        $"Id обязательного объекта '{entityName}' не был передан (= null)")
    {
    }
}