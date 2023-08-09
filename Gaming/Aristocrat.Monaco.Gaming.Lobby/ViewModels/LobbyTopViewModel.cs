namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using Prism.Mvvm;

public class LobbyTopViewModel : BindableBase
{
    private const string TopImageDefaultResourceKey = "TopBackground";
    private const string TopImageAlternateResourceKey = "TopBackgroundAlternate";

    private string? _topImageResourceKey;

    public string? TopImageResourceKey
    {
        get => _topImageResourceKey;

        set => SetProperty(ref _topImageResourceKey, value);
    }
}
