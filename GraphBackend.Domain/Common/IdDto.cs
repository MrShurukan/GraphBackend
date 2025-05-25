namespace GraphBackend.Domain.Common;

public class IdDto<TId>
{
    public TId Id { get; set; }

    public IdDto()
    {
    }

    public IdDto(TId id)
    {
        Id = id;
    }
}