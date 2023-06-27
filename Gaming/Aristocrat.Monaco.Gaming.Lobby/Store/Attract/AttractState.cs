namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using Fluxor;

[FeatureState]
public class AttractState
{
    public int CurrentAttractIndex { get; set; }

    public bool IsAttractPlaying { get; set; }

    public bool IsPrimaryLanguageSelected { get; set; }

    public bool IsAttractMode { get; set; }

    public int ConsecutiveAttractCount { get; set; }

    // public IImmutableList<string> RotateTopperImageAfterAttractVideo { get; set; } = ImmutableList<string>.Empty;

    public int AttractModeTopperImageIndex { get; set; } = -1;

    public int AttractModeTopImageIndex { get; set; } = -1;

    public string? TopAttractVideoPath { get; set; }

    public string? BottomAttractVideoPath { get; set; }

    public bool IsTopperAttractFeaturePlaying { get; set; }

    public bool IsTopAttractFeaturePlaying { get; set; }

    public bool IsBottomAttractFeaturePlaying { get; set; }
}
