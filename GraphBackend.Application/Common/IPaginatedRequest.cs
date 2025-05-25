using MediatR;

namespace GraphBackend.Application.Common;

public interface IPaginatedRequest<T> : IRequest<PaginatedList<T>>
{
}

public record PaginatedRequest<T> : IPaginatedRequest<T>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}