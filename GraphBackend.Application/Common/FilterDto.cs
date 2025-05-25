using GraphBackend.Domain.Common;

namespace GraphBackend.Application.Common;

public class FilterDto<T>
{
    public T? Value { get; set; }
}

public class ListFilterIdDto<T>
{
    public FilterModes FilterMode { get; set; }
    public List<IdDto<T>>? Value { get; set; }
}