namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using System.Collections.Immutable;
using Fluxor;
using Models;
using Redux;

[Selectors(typeof(ChooserState))]
public class ChooserSelectors
{
    public ChooserSelectors(IState<ChooserState> state)
    {
        StateSelector = new FeatureSelector<ChooserState>(state);
        GamesSelector = new PropertySelector<ChooserState, IImmutableList<GameInfo>>(StateSelector, s => s.Games);
        IsExtraLargeIconsSelector = new PropertySelector<ChooserState, bool>(StateSelector, s => s.IsExtraLargeIcons);
        GamesPerPageSelector = new PropertySelector<ChooserState, int>(StateSelector, s => s.GamesPerPage);
        ChangeGameIconLayoutSelector = new Selector<ChooserState, bool, int, int>(
            IsExtraLargeIconsSelector,
            GamesPerPageSelector,
            (lg, gp) => lg ? gp : 10);
    }

    public ISelector<ChooserState, ChooserState> StateSelector { get; }

    public ISelector<ChooserState, IImmutableList<GameInfo>> GamesSelector { get; }

    public ISelector<ChooserState, bool> IsExtraLargeIconsSelector { get; }

    public ISelector<ChooserState, int> GamesPerPageSelector { get; }

    public ISelector<ChooserState, int> ChangeGameIconLayoutSelector { get; }
}
