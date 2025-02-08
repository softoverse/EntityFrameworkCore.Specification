using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Query;

using Softoverse.Specification.Abstraction;
using Softoverse.Specification.Helpers;

namespace Softoverse.Specification.Implementation;

public class Specification<TEntity> : ISpecification<TEntity> where TEntity : class
{
    private Specification() { }

    public Specification(List<Expression<Func<TEntity, bool>>> expressions, CombineType combineType, bool asNoTracking = false, bool asSplitQuery = false)
    {
        var criteria = combineType == CombineType.And
            ? expressions.CombineWithAnd()
            : expressions.CombineWithOr();

        Criteria = criteria;
        AsNoTracking = asNoTracking;
        AsSplitQuery = asSplitQuery;
    }

    public Specification(object? primaryKey, bool asNoTracking = false, bool asSplitQuery = false)
    {
        PrimaryKey = primaryKey;
        AsNoTracking = asNoTracking;
        AsSplitQuery = asSplitQuery;
    }

    public object? PrimaryKey { get; protected set; }
    public Expression<Func<TEntity, bool>>? Criteria { get; protected set; }

    public bool AsSplitQuery { get; protected set; }
    public bool AsNoTracking { get; protected set; }

    public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = new();
    public List<string> IncludeStrings { get; } = new();

    public Expression<Func<TEntity, object>>? OrderByExpression { get; set; }
    public Expression<Func<TEntity, object>>? OrderByDescendingExpression { get; set; }

    public Expression<Func<TEntity, object>>? ProjectionExpression { get; set; }

    public Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> ExecuteUpdateExpression { get; set; }

    public void AddInclude(Expression<Func<TEntity, object>> includeExpression) => IncludeExpressions.Add(includeExpression);

    public void AddIncludeString(string includeString) => IncludeStrings.Add(includeString);

    public void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression) => OrderByExpression = orderByExpression;

    public void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression) => OrderByDescendingExpression = orderByDescendingExpression;

    public void SetProjection(Expression<Func<TEntity, object>> projectionExpression) => ProjectionExpression = projectionExpression;
    public void SetExecuteUpdateExpression(Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> executeUpdateExpression) => ExecuteUpdateExpression = executeUpdateExpression;

    public static Expression<Func<TEntity, bool>> ToConditionalExpression<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector, string value, Expression<Func<TEntity, bool>>? defaultExpression)
    {
        // If not a query string, return the default query
        if (!IsQueryString(value))
        {
            return defaultExpression ?? throw new ArgumentException($"'defaultExpression' was not being provided for the given value: {value} and also did not contain a query sting");
        }

        const string lessThan = "lt";
        const string lessThanOrEqual = "lte";
        const string greaterThan = "gt";
        const string greaterThanOrEqual = "gte";
        const string equal = "eq";
        const string notEqual = "ne";

        const string equalCaseInsensitive = "eqci";
        const string like = "like";
        const string likeCaseInsensitive = "likeci";
        const string range = "range";

        const string @in = "in";
        const string nin = "nin";

        const string inCaseInsensitive = "inci";
        const string ninCaseInsensitive = "ninci";

        const string inLike = "inlike";
        const string ninLike = "ninlike";

        const string inLikeCaseInsensitive = "inlikeci";
        const string ninLikeCaseInsensitive = "ninlikeci";

        const char splitBy = ',';

        var splitValues = value.Split(':');
        var condition = splitValues[0].ToLower();

        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;

        var targetType = typeof(TProperty);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Handle basic comparisons: 'lt', 'lte', 'gt', 'gte', 'eq', 'ne', 'eqci', 'like', 'likeci', 'range, 'in', 'nin', 'inci', 'ninci', 'inlike', 'ninlike', 'inlikeci', 'ninlikeci'
        return condition switch
        {
            lessThan or lessThanOrEqual or greaterThan or greaterThanOrEqual or equal or notEqual =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildComparisonExpression(property, condition, ConvertToType(splitValues[1], underlyingType), targetType),
                                                       parameter),

            equalCaseInsensitive when targetType == typeof(string) =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       Expression.Equal(
                                                                        Expression.Call(property, typeof(string).GetMethod(nameof (string.ToLower), Type.EmptyTypes)!),
                                                                        Expression.Constant(splitValues[1].ToLower())),
                                                       parameter),

            like =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       Expression.Call(
                                                                       property,
                                                                       typeof(string).GetMethod(nameof (string.Contains), [typeof(string)])!,
                                                                       Expression.Constant(splitValues[1])),
                                                       parameter),

            likeCaseInsensitive =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       Expression.Call(
                                                                       Expression.Call(property, typeof(string).GetMethod(nameof (string.ToLower), Type.EmptyTypes)!),
                                                                       typeof(string).GetMethod(nameof (string.Contains), [typeof(string)])!,
                                                                       Expression.Constant(splitValues[1].ToLower())),
                                                       parameter),

            range =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       Expression.AndAlso(
                                                                          BuildComparisonExpression(property, greaterThanOrEqual, ConvertToType(splitValues[1].Split(splitBy)[0], underlyingType), targetType),
                                                                          BuildComparisonExpression(property, lessThanOrEqual, ConvertToType(splitValues[1].Split(splitBy)[1], underlyingType), targetType)
                                                                         ),
                                                       parameter),

            @in =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildInExpression(property, splitValues[1].Split(splitBy), underlyingType, isNotIn: false),
                                                       parameter),

            nin =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildInExpression(property, splitValues[1].Split(splitBy), underlyingType, isNotIn: true),
                                                       parameter),

            inCaseInsensitive when targetType == typeof(string) =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildInExpression(property, splitValues[1].Split(splitBy), underlyingType, isNotIn: false, caseInsensitive: true),
                                                       parameter),

            ninCaseInsensitive when targetType == typeof(string) =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildInExpression(property, splitValues[1].Split(splitBy), underlyingType, isNotIn: true, caseInsensitive: true),
                                                       parameter),

            inLike =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildInExpression(property, splitValues[1].Split(splitBy), underlyingType, isNotIn: false, isLike: true),
                                                       parameter),

            ninLike =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildInExpression(property, splitValues[1].Split(splitBy), underlyingType, isNotIn: true, isLike: true),
                                                       parameter),

            inLikeCaseInsensitive when targetType == typeof(string) =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildInExpression(property, splitValues[1].Split(splitBy), underlyingType, isNotIn: false, isLike: true, caseInsensitive: true),
                                                       parameter),

            ninLikeCaseInsensitive when targetType == typeof(string) =>
                Expression.Lambda<Func<TEntity, bool>>(
                                                       BuildInExpression(property, splitValues[1].Split(splitBy), underlyingType, isNotIn: true, isLike: true, caseInsensitive: true),
                                                       parameter),

            _ => defaultExpression ?? throw new ArgumentException($"'defaultExpression' was not being provided for the given value: {value}")
        };

        // Local function to determine if the value is a query string
        static bool IsQueryString(string? value)
        {
            return !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value) && value.Contains(":");
        }

        // Local function to build comparison expressions
        static Expression BuildComparisonExpression(Expression property, string condition, object constantValue, Type targetType)
        {
            Func<Expression, Expression, BinaryExpression> comparison = condition switch
            {
                lessThan => Expression.LessThan,
                lessThanOrEqual => Expression.LessThanOrEqual,
                greaterThan => Expression.GreaterThan,
                greaterThanOrEqual => Expression.GreaterThanOrEqual,
                equal => Expression.Equal,
                notEqual => Expression.NotEqual,
                _ => throw new ArgumentException($"Unsupported condition: {condition}")
            };

            return comparison(property, Expression.Constant(constantValue, targetType));
        }

        // Local function to convert a string to the appropriate target type
        static object ConvertToType(string input, Type targetType)
        {
            if (targetType != typeof(DateTime) && targetType != typeof(DateTimeOffset))
            {
                return Convert.ChangeType(input, targetType);
            }

            if (DateTime.TryParse(input, out DateTime dateTimeValue))
            {
                return dateTimeValue;
            }
            throw new ArgumentException($"Invalid DateTime format for value: {input}");

        }

        // Local function to create an 'in', 'nin', 'inci', 'ninci', 'inlike', 'inlikeci', 'ninlike', 'ninlikeci' condition using HashSet<T>
        static Expression BuildInExpression(Expression property, string[] values, Type targetType, bool isNotIn, bool caseInsensitive = false, bool isLike = false)
        {
            // Convert the property to lowercase if case-insensitive comparison is needed
            if (caseInsensitive && targetType == typeof(string))
            {
                property = Expression.Call(property, nameof (string.ToLower), Type.EmptyTypes);
            }

            // For 'inlike' and 'ninlike' operations (pattern matching using 'Contains')
            if (isLike && targetType == typeof(string))
            {
                // Convert the values to the appropriate type
                var inValues = new HashSet<string>(values.Select(v => caseInsensitive ? v.ToLower() : v));

                var containsExpressions = inValues.Select(value =>
                                                              Expression.Call(property, typeof(string).GetMethod(nameof (string.Contains), [typeof(string)])!, Expression.Constant(value)) as Expression
                                                         ).ToList();

                if (!containsExpressions.Any())
                {
                    return isNotIn ? Expression.Constant(true) : Expression.Constant(false);
                }

                var combinedExpression = containsExpressions.Aggregate(Expression.OrElse);
                return isNotIn ? Expression.Not(combinedExpression) : combinedExpression;
            }
            else
            {
                // Convert the values to the appropriate type
                var inValues = new HashSet<TProperty>(values.Select(v => (TProperty)ConvertToType(v, targetType)));

                // For regular 'in' or 'nin' operations
                var inList = Expression.Constant(inValues);
                var containsMethod = typeof(HashSet<TProperty>).GetMethod(nameof (HashSet<TProperty>.Contains), [property.Type]);

                var containsExpression = Expression.Call(inList, containsMethod!, property);
                return isNotIn ? Expression.Not(containsExpression) : containsExpression;
            }
        }
    }

}