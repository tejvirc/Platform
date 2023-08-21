namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public class LobbyTopViewModel : ObservableObject
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
