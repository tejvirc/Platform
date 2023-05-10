namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using Fluxor;

[FeatureState]
public record AttractState
{
    public string? DefaultVideoPath { get; set; }

    public string? TopVideoPath { get; set; }

    public string? BottomVideoPath { get; set; }

    public bool IsAlternateLanguage { get; set; }

    public bool CurrentIndex { get; set; }

    public string? CurrentLanguage { get; set; }

    public bool IsAttractPlaying { get; set; }
}
