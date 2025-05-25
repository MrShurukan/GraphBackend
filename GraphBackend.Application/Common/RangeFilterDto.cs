namespace GraphBackend.Application.Common;

public class RangeFilterDto<T>
{
    public T From { get; set; }
    public T To { get; set; }
}