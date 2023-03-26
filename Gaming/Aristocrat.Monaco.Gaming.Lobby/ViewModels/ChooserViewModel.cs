namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using Fluxor.Selectors;
using Models;
using Store.Chooser;

public class ChooserViewModel : ObservableObject
{
    public ChooserViewModel(IStore store)
    {
        Items = store.SubscribeSelector<ChooserState, IImmutableList<ChooserItem>>(s => s.Items);
    }

    public ISelectorSubscription<IImmutableList<ChooserItem>> Items { get; set; }
}
