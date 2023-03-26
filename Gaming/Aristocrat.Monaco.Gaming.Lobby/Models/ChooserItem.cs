namespace Aristocrat.Monaco.Gaming.Lobby.Models;

using CommunityToolkit.Mvvm.ComponentModel;

public class ChooserItem : ObservableObject
{
    public long Id { get; init; }

    public int GameId { get; init; }

    public string ThemeId { get; init; }

    public string PaytableId { get; init; }

    public long Denomination { get; init; }

    public string BetOption { get; init; }
}
