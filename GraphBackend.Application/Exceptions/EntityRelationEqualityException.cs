using GraphBackend.Domain.Exceptions;

namespace GraphBackend.Application.Exceptions;

public class EntityRelationEqualityException : Forbidden403Exception
{
    public EntityRelationEqualityException(string? message = "Реляционные отношения сущности не верны")
        : base(message)
    {
    }
    
    public EntityRelationEqualityException(string entityName, string relationName, int entityId, int relationId)
        : base($"Реляционные отношения сущности '{entityName} с ID = {entityId}' не совпадают с сущностью '{relationName}' с ID = {relationId}")
    {
    }
    
    public EntityRelationEqualityException(string entityName, string relationName, Enum entityId, int relationId)
        : base($"Реляционные отношения сущности '{entityName} с ID = {Convert.ToInt32(entityId)}' не совпадают с сущностью '{relationName}' с ID = {relationId}")
    {
    }
    
    public EntityRelationEqualityException(string entityName, string relationName, int entityId, Enum relationId)
        : base($"Реляционные отношения сущности '{entityName} с ID = {entityId}' не совпадают с сущностью '{relationName}' с ID = {Convert.ToInt32(relationId)}")
    {
    }
    
    public EntityRelationEqualityException(string entityName, string relationName, Enum entityId, Enum relationId)
        : base($"Реляционные отношения сущности '{entityName} с ID = {Convert.ToInt32(entityId)}' не совпадают с сущностью '{relationName}' с ID = {Convert.ToInt32(relationId)}")
    {
    }
}