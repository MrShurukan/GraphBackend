using GraphBackend.Application.Common;
using GraphBackend.Application.Utils;
using GraphBackend.Domain.Models;
using MediatR;

namespace GraphBackend.Application.CQRS.Queries;

public record GetUsersByFilterQuery(
    FilterDto<string?>? Email,
    FilterDto<Roles?>? Role) : PaginatedRequest<UserDto>;

public class GetUsersByFilterQueryHandler(
    IApplicationContext context) : IRequestHandler<GetUsersByFilterQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(GetUsersByFilterQuery query, CancellationToken cancellationToken)
    {
        var filterManager = new FilterManager<User>(context.Users);

        var finalQuery = filterManager
            .ILike(query.Email, x => x.Email, LikeFlags.InTheMiddle)
            .Add(query.Role, x => x.Role)
            .GetFinalQueryable()
            .OrderByDescending(o => o.Id);
        
        var paginatedList =
            await finalQuery.PaginatedListEnumerableAsync<User, UserDto>(query.PageNumber, query.PageSize);

        return paginatedList;
    }
}