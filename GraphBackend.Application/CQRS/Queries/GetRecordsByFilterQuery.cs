﻿using GraphBackend.Application.Common;
using GraphBackend.Application.Utils;
using GraphBackend.Domain.Models;
using MediatR;

namespace GraphBackend.Application.CQRS.Queries;

public record GetRecordsByFilterQuery(
    FilterDto<string?>? Url,
    FilterDto<string?>? UrlWithOwner,
    FilterDto<string?>? WallOwner,
    FilterDto<string?>? PostAuthor,
    FilterDto<string?>? Text,
    FilterDto<string?>? CommentUrl,
    FilterDto<string?>? AuthorName,
    FilterDto<string?>? Classification,
    
    FilterDto<DateTime?>? FromDateTime,
    FilterDto<DateTime?>? ToDateTime) : PaginatedRequest<HeroRecordDto>;

public class GetRecordsByFilterQueryHandler(
    IApplicationContext context) : IRequestHandler<GetRecordsByFilterQuery, PaginatedList<HeroRecordDto>>
{
    public async Task<PaginatedList<HeroRecordDto>> Handle(GetRecordsByFilterQuery query, CancellationToken cancellationToken)
    {
        var finalQuery = GetFilterFromQuery(query, context.HeroRecords)
            .OrderBy(o => o.DateTime);
        
        var paginatedList =
            await finalQuery.PaginatedListEnumerableAsync<HeroRecord, HeroRecordDto>(query.PageNumber, query.PageSize);

        return paginatedList;
    }

    public static IQueryable<HeroRecord> GetFilterFromQuery(GetRecordsByFilterQuery query,
        IQueryable<HeroRecord> heroRecords)
    {
        var filterManager = new FilterManager<HeroRecord>(heroRecords);

        HeroRecordClassification? classification = null; 
        if (query.Classification?.Value is not null)
        {
            classification = (HeroRecordClassification)int.Parse(query.Classification.Value);
        }
        
        var finalQuery = filterManager
            .AddQuery(query.Classification, x => x.Classification == classification)
            .ILike(query.Url, x => x.Url, LikeFlags.InTheMiddle)
            .ILike(query.UrlWithOwner, x => x.UrlWithOwner, LikeFlags.InTheMiddle)
            .ILike(query.WallOwner, x => x.WallOwner, LikeFlags.InTheMiddle)
            .ILike(query.PostAuthor, x => x.PostAuthor, LikeFlags.InTheMiddle)
            .ILike(query.Text, x => x.Text, LikeFlags.InTheMiddle)
            .ILike(query.CommentUrl, x => x.CommentUrl, LikeFlags.InTheMiddle)
            .ILike(query.AuthorName, x => x.AuthorName, LikeFlags.InTheMiddle)
            .AddQuery(query.FromDateTime, x => x.DateTime >= query.FromDateTime!.Value.Value!.ToUniversalTime())
            .AddQuery(query.ToDateTime, x => x.DateTime <= query.ToDateTime!.Value!.Value.ToUniversalTime())
            .GetFinalQueryable();

        return finalQuery;
    }
}