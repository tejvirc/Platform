namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;
using System.Linq.Expressions;

public sealed class PropertySelector<TState, TProperty> : MemorizedSelector<TState, TState, TProperty> where TState : class
{
    public PropertySelector(ISelector<TState, TState> stateSelector, Expression<Func<TState, TProperty>> propertyExpr)
        : base(stateSelector, s => GetValue(s, propertyExpr))
    {
        if (propertyExpr.Body is not MemberExpression)
        {
            throw new ArgumentException(@"Only member expressions supported",  nameof(propertyExpr));
        }
    }

    private static TProperty GetValue(TState state, Expression<Func<TState, TProperty>> propertyExpr)
    {
        var stateParamExpr = Expression.Parameter(typeof(TState), "state");
        var lambdaExpr = Expression.Lambda<Func<TState, TProperty>>(propertyExpr, stateParamExpr);

        return lambdaExpr.Compile().Invoke(state);
    }
}
