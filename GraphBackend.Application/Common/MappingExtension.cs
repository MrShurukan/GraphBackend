namespace GraphBackend.Application.Common;

public static class MappingExtensions
{
    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TSource, TDestination>(this IQueryable<TSource> queryable, int pageNumber, int pageSize) where TDestination : class
        => PaginatedList<TDestination>.CreateProjectedListAsync<TSource, TDestination>(queryable, pageNumber, pageSize);
    
    public static Task<PaginatedList<TDestination>> PaginatedListEnumerableAsync<TSource, TDestination>(this IQueryable<TSource> queryable, int pageNumber, int pageSize) where TDestination : class
        => PaginatedList<TDestination>.CreateProjectedListEnumerableAsync<TSource, TDestination>(queryable, pageNumber, pageSize);

    
    public static Task<IQueryable<T>> PaginatedQueryableAsync<T>(this IQueryable<T> queryable, int pageNumber, int pageSize) where T : class
        => PaginatedList<T>.CreateQueryableAsync(queryable, pageNumber, pageSize);
    
    public static Task<PaginatedList<T>> PaginatedQueryableToListAsync<T>(this IQueryable<T> queryable, int pageNumber, int pageSize) where T : class
        => PaginatedList<T>.CreateQueryableToListAsync(queryable, pageNumber, pageSize);
}
