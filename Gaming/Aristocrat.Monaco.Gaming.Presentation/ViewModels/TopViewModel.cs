namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using Aristocrat.Monaco.Gaming.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;

public class TopViewModel : ObservableObject
{
    private const string TopImageDefaultResourceKey = "TopBackground";
    private const string TopImageAlternateResourceKey = "TopBackgroundAlternate";

    private string? _topImageResourceKey;

    public string TopTitle => GamingConstants.TopWindowTitle;

    public string? TopImageResourceKey
    {
        get => _topImageResourceKey;

        set => SetProperty(ref _topImageResourceKey, value);
    }
}
