namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using Contracts.Models;

public class AddInfoOverlayTextAction
{
    public AddInfoOverlayTextAction(string text, InfoLocation location)
    {
        Text = text;
        Location = location;
    }

    public string Text { get; }

    public InfoLocation Location { get; }
}
