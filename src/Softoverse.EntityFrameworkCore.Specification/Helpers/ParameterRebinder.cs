using System.Linq.Expressions;

namespace Softoverse.EntityFrameworkCore.Specification.Helpers;

public class ParameterRebinder : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;

    public ParameterRebinder(ParameterExpression parameter)
    {
        _parameter = parameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return base.VisitParameter(_parameter);
    }

    public static Expression ReplaceParameters(ParameterExpression parameter, Expression expression)
    {
        return new ParameterRebinder(parameter).Visit(expression);
    }
}
