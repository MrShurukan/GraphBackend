using System.Linq.Expressions;
using GraphBackend.Application.Exceptions;
using GraphBackend.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.Extensions;

public static class DbSetExtensions
{ 
    public static async Task<T?> FindAsyncIntId<T>(this DbSet<T> dbSet, int id, CancellationToken token = default)
        where T : class
    {
        return await dbSet.FindAsync(new object?[] { id }, cancellationToken: token);
    }
    
    public static async Task<T?> FindAsyncIntId<T>(this DbSet<T> dbSet, Enum id, CancellationToken token = default)
        where T : class
    {
        return await dbSet.FindAsync(new object?[] { id }, cancellationToken: token);
    }

    /// <summary>
    /// Производит поиск по сущности в <see cref="DbSet{T}"/> и кидает ошибку:
    /// 
    /// 1) Если id было null
    /// 2) Если сущность не найдена 
    /// </summary>
    /// <param name="dbSet">Текущий <see cref="DbSet{T}"/></param>
    /// <param name="id">Id сущности. Принимает null</param>
    /// <param name="token">CancellationToken</param>
    /// <param name="customName">Возможность дать имя сущности, которое ищется (используется для сообщения Exception)</param>
    /// <typeparam name="T">Тип объекта в DbSet</typeparam>
    /// <returns>Найденную сущность</returns>
    /// <exception cref="RequiredEntityIdWasNullException">Переданный ID был null</exception>
    /// <exception cref="RequiredEntityWasNotFoundException">Объект не был найден в базе</exception>
    public static async Task<T> FindRequiredAsyncIntId<T>(this DbSet<T> dbSet, int? id, CancellationToken token = default, string? customName = null)
        where T : class
    {
        if (id is null)
            throw new RequiredEntityIdWasNullException(customName ?? typeof(T).Name);

        return await dbSet.FindAsyncIntId((int)id, token)
               ?? throw new RequiredEntityWasNotFoundException(customName ?? typeof(T).Name, (int)id);
    }
    
    /// <summary>
    /// Производит Any(x => x.Id == id) по сущности в <see cref="DbSet{T}"/> и кидает ошибку:
    /// 
    /// 1) Если id было null
    /// 2) Если сущность не найдена 
    /// </summary>
    /// <param name="dbSet">Текущий <see cref="DbSet{T}"/></param>
    /// <param name="id">Id сущности. Принимает null</param>
    /// <param name="token">CancellationToken</param>
    /// <param name="customName">Возможность дать имя сущности, которое ищется (используется для сообщения Exception)</param>
    /// <typeparam name="T">Тип объекта в DbSet</typeparam>
    /// <exception cref="RequiredEntityIdWasNullException">Переданный ID был null</exception>
    /// <exception cref="RequiredEntityWasNotFoundException">Объект не был найден в базе</exception>
    public static async Task EnsureExistsAsync<T>(this DbSet<T> dbSet, int? id, CancellationToken token = default, string? customName = null)
        where T : BaseEntity
    {
        if (id is null)
            throw new RequiredEntityIdWasNullException(customName ?? typeof(T).Name);

        if (!await dbSet.AnyAsync(x => x.Id == (int)id, token))
               throw new RequiredEntityWasNotFoundException(customName ?? typeof(T).Name, (int)id);
    }
    
    /// <summary>
    /// Производит поиск по сущности в <see cref="DbSet{T}"/> и кидает ошибку:
    ///
    /// 1) Если id было null
    /// 2) Если сущность не найдена 
    /// </summary>
    /// <param name="dbSet">Текущий <see cref="DbSet{T}"/></param>
    /// <param name="id">Id сущности (в виде Enum). Принимает null</param>
    /// <param name="token">CancellationToken</param>
    /// <typeparam name="T">Тип объекта в DbSet</typeparam>
    /// <returns>Найденную сущность</returns>
    /// <exception cref="RequiredEntityIdWasNullException">Переданный ID был null</exception>
    /// <exception cref="RequiredEntityWasNotFoundException">Объект не был найден в базе</exception>
    public static async Task<T> FindRequiredAsyncIntId<T>(this DbSet<T> dbSet, Enum? id, CancellationToken token = default, string? customName = null)
        where T : class
    {
        if (id is null)
            throw new RequiredEntityIdWasNullException(typeof(T).Name);

        return await dbSet.FindAsyncIntId(id, token)
               ?? throw new RequiredEntityWasNotFoundException(customName ?? typeof(T).Name, Convert.ToInt32(id));
    }

    public static IQueryable<TSource> WhereByIds<TSource, TId>(this IQueryable<TSource> queryable, List<TId> ids,
        string? customName = null) where TSource : class
    {
        var parameterX = Expression.Parameter(typeof(TSource), "x");
       
        var callContainsExpression = Expression.Call(
            Expression.Constant(ids),
            typeof(List<TId>).GetMethod(nameof(List<TId>.Contains))!,
            Expression.Property(parameterX, "Id")
        );

        var lambda = Expression.Lambda<Func<TSource, bool>>(
            callContainsExpression, parameterX
        );

        var entities = queryable.Where(lambda);

        var getIdExpression = Expression.Property(parameterX, "Id");
        var getIdLambda = Expression.Lambda<Func<TSource, TId>>(getIdExpression, parameterX);
        
        // Делаем разность коллекций по Id
        var except = ids.Except(entities.Select(getIdLambda)).ToList();
        // Если разность имееть хотя бы 1 элемент, значит что-то не нашлось. И это что-то записано в except
        if (except.Any())
            throw new EntityWasNotFoundException($"Сущность(-и) '{customName ?? typeof(TSource).Name}' с Id [{string.Join(", ", except)}] не найдены");

        return entities;
    }
    public static IQueryable<TSource> WhereByIds<TSource, TId>(this IQueryable<TSource> queryable, IEnumerable<TId> ids) where TSource : class
    {
        return queryable.WhereByIds(ids.ToList());
    }
}