using System.Linq.Expressions;

namespace CallRecordIntelligence.EF.Services;

public static class PredicateBuilder
{
    /// <summary>
    /// Combines two expression predicates with a logical AND.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="left">The left-hand side predicate.</param>
    /// <param name="right">The right-hand side predicate.</param>
    /// <returns>A new expression predicate combining the two with AND.</returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Body);

        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(leftBody, rightBody), parameter);
    }

    /// <summary>
    /// Combines two expression predicates with a logical OR.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="left">The left-hand side predicate.</param>
    /// <param name="right">The right-hand side predicate.</param>
    /// <returns>A new expression predicate combining the two with OR.</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Body);

        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(leftBody, rightBody), parameter);
    }

    /// <summary>
    /// Helper visitor to replace parameters in an expression tree.
    /// This is a standard technique when combining expression trees with different parameters.
    /// </summary>
    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;
        
        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        /// <summary>
        /// Visits a node in the expression tree.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The modified expression node.</returns>
        public override Expression Visit(Expression node)
        {
            if (node == _oldValue)
            {
                return _newValue;
            }
            return base.Visit(node);
        }
    }
}
