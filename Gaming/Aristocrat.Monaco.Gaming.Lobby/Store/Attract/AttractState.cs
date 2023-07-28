namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using System.Collections.Immutable;
using Fluxor;
using UI.Models;

[FeatureState]
public record AttractState
{
    public IImmutableList<AttractVideoDetails> AttractVideos { get; set; } = ImmutableList<AttractVideoDetails>.Empty;

    public int CurrentAttractIndex { get; set; }

    public bool IsAttractPlaying { get; set; }

    public bool IsPrimaryLanguageSelected { get; set; }

    public bool IsAttractMode { get; set; }

    public int ConsecutiveAttractCount { get; set; }

    public int AttractModeTopperImageIndex { get; set; } = -1;

    public int AttractModeTopImageIndex { get; set; } = -1;

    public string? TopImageResourceKey { get; set; }

    public string? TopperImageResourceKey { get; set; }

    public bool IsAlternateTopImageActive { get; set; }

    public bool IsAlternateTopperImageActive { get; set; }

    public string? TopAttractVideoPath { get; set; }

    public string? BottomAttractVideoPath { get; set; }

    public bool IsTopperAttractFeaturePlaying { get; set; }

    public bool IsTopAttractFeaturePlaying { get; set; }

    public bool IsBottomAttractFeaturePlaying { get; set; }

    public bool NextAttractModeLanguageIsPrimary { get; set; }

    public bool LastInitialAttractModeLanguageIsPrimary { get; set; }
}
