using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using GraphBackend.Application.Common;
using GraphBackend.Domain.Common;
using GraphBackend.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.Utils;

/// <summary>
/// Вспомогательный класс для более простого создания фильтров из базы. Производит магию с Queryable и Expression, чтобы быть совместимым
/// с EF Core
/// </summary>
/// <typeparam name="TEntity">Сущность, по которой производится фильтрование в базе</typeparam>
public class FilterManager<TEntity> where TEntity : class
{
    private IQueryable<TEntity> _queryable;
    private readonly MethodInfo _efFunctionILike = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
        "ILike", 
        new[] {typeof(Microsoft.EntityFrameworkCore.DbFunctions), 
            typeof(string), 
            typeof(string)})!;
    private readonly MethodInfo _stringFormat = typeof(string).GetMethod("Format", new [] { typeof(string), typeof(object) })!;

    public FilterManager(IQueryable<TEntity> queryable)
    {
        _queryable = queryable ?? throw new ArgumentNullException(nameof(queryable));
    }

    public FilterManager<TEntity> Add<TProperty>(TProperty property, Expression<Func<TEntity, TProperty>> pointerExpression)
    {
        if (property is null) return this;
        var body = pointerExpression.Body;
        
        if (body.NodeType is ExpressionType.Convert)
        {
            body = ((UnaryExpression)body).Operand;
        }

        var constant = Expression.Convert(Expression.Constant(property), body.Type);
        var equalExpression = Expression.Equal(body, constant);
        var result = Expression.Lambda<Func<TEntity, bool>>(equalExpression, pointerExpression.Parameters);
        
        _queryable = _queryable.Where(result);

        return this;
    }

    public FilterManager<TEntity> Add(Enum? property, Expression<Func<TEntity, int>> pointerExpression)
    {
        if (property is null) return this;
        var propertyInt = Convert.ToInt32(property);
        return Add(propertyInt, pointerExpression);
    }
    
    /// <summary>
    /// Добавить фильтр через FilterDto. Эта перегрузка не подразумевает, что значение FilterDto.Value будет null (будет выброшен Exception).<br/><br/>
    /// Сам FilterDto может быть null, тогда поиск производится не будет
    /// </summary>
    /// <param name="property">FilterDto для поиска</param>
    /// <param name="pointerExpression">Указание на свойство у сущности</param>
    /// <typeparam name="TProperty">Тип сущности</typeparam>
    /// <returns>Фильтр менеджер с добавленным Query</returns>
    public FilterManager<TEntity> Add<TProperty>(FilterDto<TProperty>? property, Expression<Func<TEntity, TProperty>> pointerExpression)
    {
        if (property is not null && property.Value is null)
        {
            var body = pointerExpression.Body;

            var memberExpression = body.NodeType is ExpressionType.Convert 
                ? (MemberExpression)((UnaryExpression)body).Operand 
                : (MemberExpression)body;
            
            throw new UnexpectedNullValueFilterManagerException(memberExpression.Member.Name);
        }
        
        HandleFilterDto(body =>
        {
            var constant = Expression.Convert(Expression.Constant(property!.Value), body.Type);
            var equalExpression = Expression.Equal(body, constant);
            var result = Expression.Lambda<Func<TEntity, bool>>(equalExpression, pointerExpression.Parameters);

            return result;
        }, property!, pointerExpression);

        return this;
    }
    
    /// <summary>
    /// Добавить фильтр через FilterDto. Эта перегрузка нужна, если есть возможность, что придёт запрос на "пустое" значение.<br/><br/>
    /// При этом будет произведен поиск по соответствию из массива <see cref="defaultValues"/>.
    /// Сам FilterDto может быть null, тогда поиск производится не будет
    /// </summary>
    /// <param name="property">FilterDto для поиска</param>
    /// <param name="pointerExpression">Указание на свойство у сущности</param>
    /// <param name="defaultValues">Массив "пустых" значений - значений по умолчанию</param>
    /// <typeparam name="TProperty">Тип сущности</typeparam>
    /// <returns></returns>
    public FilterManager<TEntity> Add<TProperty>(FilterDto<TProperty>? property, Expression<Func<TEntity, TProperty>> pointerExpression, params TProperty[] defaultValues)
    {
        HandleFilterDto(body =>
        {
            var constant = Expression.Convert(Expression.Constant(property!.Value), body.Type);
            var equalExpression = Expression.Equal(body, constant);
            var result = Expression.Lambda<Func<TEntity, bool>>(equalExpression, pointerExpression.Parameters);

            return result;
        }, property!, pointerExpression, defaultValues);
        
        return this;
    }
    
    /// <summary>
    /// Добавить фильтр через ListFilterDto. Это работает как или...или для всего списка.<br/><br/>
    /// Сам ListFilterDto может быть null, тогда поиск производится не будет
    /// </summary>
    /// <param name="property">ListFilterDto для поиска</param>
    /// <param name="pointerExpression">Указание на свойство у сущности</param>
    /// <typeparam name="TProperty">Тип сущности</typeparam>
    /// <returns></returns>
    public FilterManager<TEntity> Add<TProperty>(ListFilterIdDto<TProperty>? property, Expression<Func<TEntity, TProperty>> pointerExpression)
    {
        HandleFilterDto(body =>
        {
            Expression? orAccumulator = null;

            // Гарантия, что массив не пустой благодаря HandleFilterDto
            foreach (var idDto in property!.Value!)
            {
                var value = idDto.Id;
                var constant = Expression.Convert(Expression.Constant(value), body.Type);

                var equalExpression = Expression.Equal(body, constant);
                if (orAccumulator is null)
                {
                    orAccumulator = equalExpression;
                }
                else
                {
                    orAccumulator = Expression.OrElse(orAccumulator, equalExpression);
                }
            }
            
            var result = Expression.Lambda<Func<TEntity, bool>>(orAccumulator!, pointerExpression.Parameters);

            return result;
        }, property!, pointerExpression);
        
        return this;
    }
    
    /// <summary>
    /// Добавить фильтр через ListFilterDto. Это работает как или...или для всего списка.<br/><br/>
    /// Сам ListFilterDto может быть null, тогда поиск производится не будет
    /// </summary>
    /// <param name="property">ListFilterDto для поиска</param>
    /// <param name="pointerExpression">Указание на свойство у сущности</param>
    /// <typeparam name="TProperty">Тип сущности</typeparam>
    /// <returns></returns>
    /// TODO: функционал для поиска списков по спискам, черновик
    // public FilterManager<TEntity> Add<TProperty, TEntityList>(ListFilterIdDto<TProperty>? property, Expression<Func<TEntity, ICollection<TEntityList>>> pointerExpressionList, 
    //     Expression<Func<TEntityList, TProperty>> pointerExpressionPropety)
    // {
    //     
    // }
    
    
    public FilterManager<TEntity> AddTpcTypeConstraint(Type? tpcTypeToCast)
    {
        if (tpcTypeToCast is null) return this;

        var ofTypeFunction = typeof(Queryable)
            .GetMethod(nameof(Queryable.OfType), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(tpcTypeToCast);

        var castFunction = typeof(Queryable)
            .GetMethod(nameof(Queryable.Cast), BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(typeof(TEntity));

        _queryable = (IQueryable<TEntity>)castFunction.Invoke(null, [
            ofTypeFunction.Invoke(null, [_queryable])
        ])!;
        
        return this;
    }
    
    public FilterManager<TEntity> AddTpcTypeConstraintIf<TProperty, TTpcType>(FilterDto<TProperty?>? property, TProperty value)
        where TProperty : struct 
        where TTpcType : TEntity
    {
        if (property is null) return this;
        if (property.Value?.Equals(value) ?? false)
        {
            _queryable = _queryable.OfType<TTpcType>().Cast<TEntity>();
        }
        
        return this;
    }
    
    public FilterManager<TEntity> AddTpcTypeConstraintIfNot<TProperty, TTpcType>(FilterDto<TProperty?>? property, TProperty value)
        where TProperty : struct 
        where TTpcType : TEntity
    {
        if (property is null) return this;
        if ((!property.Value?.Equals(value)) ?? false)
        {
            _queryable = _queryable.OfType<TTpcType>().Cast<TEntity>();
        }
        
        return this;
    }
    
    public FilterManager<TEntity> Add<TProperty>(RangeFilterDto<TProperty>? property, Expression<Func<TEntity, TProperty>> pointerExpression)
    {
        if (property is null) return this;
        var body = pointerExpression.Body;
        
        if (body.NodeType is ExpressionType.Convert)
        {
            body = ((UnaryExpression)body).Operand;
        }
        
        var leftRange = Expression.Convert(Expression.Constant(property.From), body.Type);
        var rightRange = Expression.Convert(Expression.Constant(property.To), body.Type);

        var rangeExpression = Expression.AndAlso(
            Expression.GreaterThanOrEqual(body, leftRange),
            Expression.LessThanOrEqual(body, rightRange)
        );
        
        var result = Expression.Lambda<Func<TEntity, bool>>(rangeExpression, pointerExpression.Parameters);

        _queryable = _queryable.Where(result);

        return this;
    }

    public FilterManager<TEntity> If(bool? condition, Expression<Func<TEntity, bool>> expressionIfTrue)
    {
        if (condition is not true) return this;
        
        _queryable = _queryable.Where(expressionIfTrue);

        return this;
    }
    
    public FilterManager<TEntity> If(bool? condition, Expression<Func<TEntity, bool>> expressionIfTrue, Expression<Func<TEntity, bool>> expressionIfFalse)
    {
        if (condition is null) return this;

        _queryable = condition is true ? _queryable.Where(expressionIfTrue) : _queryable.Where(expressionIfFalse); 

        return this;
    }
    
    public FilterManager<TEntity> AddQuery<TProperty>(TProperty property, Expression<Func<TEntity, bool>> expression)
    {
        if (property is null) return this;
        _queryable = _queryable.Where(expression);

        return this;
    }

    public FilterManager<TEntity> AddQuery<TProperty>(FilterDto<TProperty?>? property, 
        Expression<Func<TEntity, bool>> expressionIfNotEmpty, Expression<Func<TEntity, bool>>? expressionIfEmpty)
    {
        var finalExpression = CheckFilterDto(property, expressionIfNotEmpty, expressionIfEmpty);
        if (finalExpression is null) return this;

        _queryable = _queryable.Where(finalExpression);
        
        return this;
    }
    
    public FilterManager<TEntity> AddQuery(Expression<Func<TEntity, bool>> expression)
    {
        _queryable = _queryable.Where(expression);
        return this;
    }

    public FilterManager<TEntity> ILike<TProperty>(FilterDto<TProperty>? property, Expression<Func<TEntity, TProperty>> pointerExpression, LikeFlags likeFlag, params TProperty[] defaultValues)
    {
        HandleFilterDto((body) =>
        {
            var pattern = likeFlag switch
            {
                LikeFlags.StartsWith => "{0}%",
                LikeFlags.InTheMiddle => "%{0}%",
                LikeFlags.EndsWith => "%{0}",
                _ => throw new ArgumentOutOfRangeException(nameof(likeFlag), likeFlag, null)
            };
            
            var constant = Expression.Convert(Expression.Constant(property!.Value), body.Type);
            var stringStatic = Expression.Call(_stringFormat!, Expression.Constant(pattern), constant);
            var staticCall = Expression.Call(_efFunctionILike,
                Expression.Property(null, typeof(EF).GetProperty("Functions")!), body, stringStatic);
            
            var lambda = Expression.Lambda<Func<TEntity, bool>>(staticCall, pointerExpression.Parameters);
            return lambda;
        }, property!, pointerExpression, defaultValues);

        return this;
    }

    public FilterManager<TEntity> Any<TProperty>(FilterDto<TProperty?>? property, Expression<Func<TEntity, IEnumerable<TProperty>>> pointerExpression)
        where TProperty : struct
    {
        HandleFilterDto((body) =>
        {
            var parameter = Expression.Parameter(typeof(TProperty), "z");
            var anySelector = Expression.Equal(parameter, Expression.Constant(property!.Value));
            var anySelectorLambda = Expression.Lambda<Func<TProperty, bool>>(anySelector, parameter);

            var any = Expression.Call(
                typeof(Enumerable), "Any", new[] { typeof(TProperty) }, body, anySelectorLambda);

            var lambda = Expression.Lambda<Func<TEntity, bool>>(any, pointerExpression.Parameters);

            return lambda;
        }, property, pointerExpression);

        return this;
    }

    public FilterManager<TEntity> IsSubarrayByIds<TProperty>(FilterDto<IEnumerable<TProperty>?>? propertyEnumerable,
        Expression<Func<TEntity, IEnumerable<TProperty>>> pointerExpression, string arrayName) where TProperty : struct
    {
        HandleFilterDto((body) =>
        {
            Expression? andExpression = null;
            
            foreach (var property in propertyEnumerable!.Value!)
            {
                var constant = Expression.Constant(property);
                var containsExpression = Expression.Call(
                    typeof(Enumerable), "Contains", new[] { typeof(TProperty) }, body, constant);

                if (andExpression is null)
                    andExpression = containsExpression;
                else
                    andExpression = Expression.AndAlso(containsExpression, andExpression);
            }

            if (andExpression is null)
                throw new BadRequest400Exception($"Для массива '{arrayName}' не указаны элементы");
            
            var lambda = Expression.Lambda<Func<TEntity, bool>>(
                andExpression, pointerExpression.Parameters);
            
            return lambda;

        }, propertyEnumerable, pointerExpression);
    
        return this;
    }

    public FilterManager<TEntity> IsSubarrayByIds<TProperty>(FilterDto<IEnumerable<IdDto<TProperty>>?>? propertyEnumerable,
        Expression<Func<TEntity, IEnumerable<TProperty>>> pointerExpression, string arrayName) where TProperty : struct
    {
        FilterDto<IEnumerable<TProperty>?>? finalPropertyEnumerable = null;
        if (propertyEnumerable is not null)
        {
            finalPropertyEnumerable = new FilterDto<IEnumerable<TProperty>?> { Value = null };
            
            if (propertyEnumerable.Value is not null)
            {
                finalPropertyEnumerable.Value = propertyEnumerable.Value.Select(x => x.Id);
            }
        }

        return IsSubarrayByIds(finalPropertyEnumerable, pointerExpression, arrayName);
    }

    private Expression<Func<TEntity, bool>>? CheckFilterDto<TProperty>(FilterDto<TProperty?>? property, 
        Expression<Func<TEntity, bool>> notEmptySearch, Expression<Func<TEntity, bool>>? emptySearch = null)
    {
        if (property is null) return null;

        if (property.Value is not null) return notEmptySearch;
        if (emptySearch is not null) return emptySearch;
        
        throw new InternalError500Exception($"Ошибка сервера: не указано поведение пустого поиска для '{property.GetType().GetGenericArguments()[0].Name}'");
    }
    
    /// <summary>
    /// Функция гарантирует, что property будет не null, если функция func будет вызвана
    /// </summary>
    /// <param name="func"></param>
    /// <param name="property"></param>
    /// <param name="pointerExpression"></param>
    /// <param name="defaultValues"></param>
    /// <typeparam name="TProperty"></typeparam>
    /// <exception cref="InternalError500Exception"></exception>
    private void HandleFilterDto<TProperty>(Func<Expression, Expression<Func<TEntity, bool>>> func, FilterDto<TProperty?>? property, 
        Expression<Func<TEntity, TProperty>> pointerExpression, params TProperty[] defaultValues)
    {
        if (property is null) return;
        
        var body = pointerExpression.Body;
        
        if (body.NodeType is ExpressionType.Convert)
        {
            body = ((UnaryExpression)body).Operand;
        }

        if (property.Value is not null)
        {
            _queryable = _queryable.Where(func(body));
        }
        else
        {
            if (defaultValues.Length == 0)
            {
                if (typeof(TProperty).IsAssignableTo(typeof(IEnumerable)))
                {
                    var countCall = Expression.Call(
                        typeof(Enumerable), "Count", new[] { typeof(TProperty).GenericTypeArguments[0] }, 
                        body);
                    var expression = Expression.Equal(countCall, Expression.Constant(0));
                    var lambdaEmpty = Expression.Lambda<Func<TEntity, bool>>(expression, pointerExpression.Parameters);
                    
                    _queryable = _queryable.Where(lambdaEmpty);
                    return;
                }
                
                var propertyName = property.GetType().GetGenericArguments()[0].Name;
                throw new InternalError500Exception($"Ошибка сервера: не указано пустое значение для '{propertyName}'");
            }
            
            BinaryExpression? finalExpression = null;
            foreach (var defaultValue in defaultValues)
            {
                var constant = Expression.Convert(Expression.Constant(defaultValue), body.Type);
                var equal = Expression.Equal(body, constant);
                
                finalExpression = finalExpression is null 
                    ? equal
                    : Expression.OrElse(equal, finalExpression);
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(finalExpression!, pointerExpression.Parameters);
            _queryable = _queryable.Where(lambda);
        }
    }
    
    /// <summary>
    /// Функция гарантирует, что property будет не null, если функция func будет вызвана.
    /// Также гарантирует, что массив не пустой (содержит хотя бы один элемент)
    /// </summary>
    /// <param name="func"></param>
    /// <param name="property"></param>
    /// <param name="pointerExpression"></param>
    /// <typeparam name="TProperty"></typeparam>
    /// <exception cref="InternalError500Exception"></exception>
    private void HandleFilterDto<TProperty>(Func<Expression, Expression<Func<TEntity, bool>>> func, ListFilterIdDto<TProperty?>? property, 
        Expression<Func<TEntity, TProperty>> pointerExpression)
    {
        if (property is null) return;
        
        var body = pointerExpression.Body;
        
        if (body.NodeType is ExpressionType.Convert)
        {
            body = ((UnaryExpression)body).Operand;
        }

        // is not null && Count > 0
        if (property.Value is { Count: > 0 })
        {
            _queryable = _queryable.Where(func(body));
        }
    }
    
    private void HandleFilterDto<TProperty>(Func<Expression, Expression<Func<TEntity, bool>>> func, FilterDto<TProperty?>? property, 
        Expression<Func<TEntity, IEnumerable<TProperty>>> pointerExpression) where TProperty : struct
    {
        if (property is null) return;
        
        var body = pointerExpression.Body;
        
        if (body.NodeType is ExpressionType.Convert)
        {
            body = ((UnaryExpression)body).Operand;
        }

        if (property.Value is not null)
        {
            _queryable = _queryable.Where(func(body));
        }
        else
        {
            var count = Expression.Call(typeof(Enumerable), "Count", new[] { typeof(TProperty) }, body);
            var expression = Expression.Equal(count, Expression.Constant(0));

            var lambda = Expression.Lambda<Func<TEntity, bool>>(expression, pointerExpression.Parameters);
            _queryable = _queryable.Where(lambda);
        }
    }

    public IQueryable<TEntity> GetFinalQueryable()
    {
        // _isFinilized = true;
        return _queryable;
    }
}

public class FilterManagerWasNeverFinalizedException : Exception
{
    public FilterManagerWasNeverFinalizedException(string? message = 
        "[Внутренняя ошибка] FilterManager был создан, но не был использован до его деструкции") : base(message)
    {
    }
}

public class UnexpectedNullValueFilterManagerException : BadRequest400Exception
{
    public UnexpectedNullValueFilterManagerException(string valueName)
        : base($"Для поля '{valueName}' нет пустого значения, но был произведен соответствующий запрос")
    {
    }
}

public enum LikeFlags
{
    StartsWith,
    InTheMiddle,
    EndsWith
}