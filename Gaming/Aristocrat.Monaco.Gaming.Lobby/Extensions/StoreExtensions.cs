namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using Fluxor;
using Fluxor.Selectors;

public static class StoreExtensions
{
    public static ISelectorSubscription<TResult> SubscribeSelector<TFeatureState, TResult>(
        this IStore store,
        Func<TFeatureState, TResult> projector)
    {
        var state = SelectorFactory.CreateFeatureSelector<TFeatureState>();
        var selectItems = SelectorFactory.CreateSelector(state, projector);
        return store.SubscribeSelector(selectItems);
    }
}
