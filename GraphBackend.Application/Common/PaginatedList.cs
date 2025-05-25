using Mapster;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.Common;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = Math.Abs((int)Math.Ceiling(count / (double)pageSize));
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static async Task<PaginatedList<TDestination>> CreateProjectedListAsync<TSource, TDestination>(IQueryable<TSource> source, int pageNumber, int pageSize)
    {
        var (queryable, count) = await SkipTakeAsync(source, pageNumber, pageSize).ConfigureAwait(false);
        
        var items = await queryable.ProjectToType<TDestination>().ToListAsync();
        
        return new PaginatedList<TDestination>(items, count, pageNumber, pageSize);
    }
    
    public static PaginatedList<TDestination> CreateProjectedListEnumerable<TSource, TDestination>(IEnumerable<TSource> source, int pageNumber, int pageSize)
    {
        var (enumerable, count) = SkipTakeEnumerable(source, pageNumber, pageSize);
        
        var sourceList = enumerable.ToList();
        var items = sourceList.Adapt<List<TSource>, List<TDestination>>();
        
        return new PaginatedList<TDestination>(items, count, pageNumber, pageSize);
    }
    
    public static async Task<PaginatedList<TDestination>> CreateProjectedListEnumerableAsync<TSource, TDestination>(IQueryable<TSource> source, int pageNumber, int pageSize)
    {
        var (queryable, count) = await SkipTakeAsync(source, pageNumber, pageSize).ConfigureAwait(false);
        
        var sourceList = await queryable.ToListAsync();
        var items = sourceList.Adapt<List<TSource>, List<TDestination>>();
        
        return new PaginatedList<TDestination>(items, count, pageNumber, pageSize);
    }
    
    public static async Task<PaginatedList<T>> CreateQueryableToListAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var (queryable, count) = await SkipTakeAsync(source, pageNumber, pageSize).ConfigureAwait(false);
        var items = await queryable.ToListAsync();
        
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
    
    public static async Task<IQueryable<T>> CreateQueryableAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var (queryable, _) = await SkipTakeAsync(source, pageNumber, pageSize).ConfigureAwait(false);
        return queryable;
    }
    
    public static PaginatedList<T> CreateEnumerable(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var (enumerable, count) = SkipTake(source, pageNumber, pageSize);
        
        return new PaginatedList<T>(enumerable.ToList(), count, pageNumber, pageSize);
    }

    private static async Task<(IQueryable<TSource>, int)> SkipTakeAsync<TSource>(IQueryable<TSource> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var queryable = source.Skip((pageNumber - 1) * pageSize);
        if(pageSize != -1)
            queryable = queryable.Take(pageSize);

        return (queryable, count);
    }
    
    private static (IEnumerable<TSource>, int) SkipTake<TSource>(IEnumerable<TSource> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var queryable = source.Skip((pageNumber - 1) * pageSize);
        if(pageSize != -1)
            queryable = queryable.Take(pageSize);

        return (queryable, count);
    }
    
    private static (IEnumerable<TSource>, int) SkipTakeEnumerable<TSource>(IEnumerable<TSource> source, int pageNumber, int pageSize)
    {
        var enumerable = source as TSource[] ?? source.ToArray();
        
        var count = enumerable.Length;
        var queryable = enumerable.Skip((pageNumber - 1) * pageSize);
        if(pageSize != -1)
            queryable = queryable.Take(pageSize);

        return (queryable, count);
    }
}