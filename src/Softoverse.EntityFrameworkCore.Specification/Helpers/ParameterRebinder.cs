using System.Linq.Expressions;

namespace Softoverse.EntityFrameworkCore.Specification.Helpers;

public class ParameterRebinder : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public ParameterRebinder(ParameterExpression newParameter, ParameterExpression oldParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }

    public static Expression ReplaceParameters(ParameterExpression newParameter, ParameterExpression oldParameter, Expression expression)
    {
        return new ParameterRebinder(newParameter, oldParameter).Visit(expression);
    }
}
