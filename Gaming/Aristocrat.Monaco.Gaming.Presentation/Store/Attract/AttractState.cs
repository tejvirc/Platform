namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attract;

using System.Collections.Generic;
using System.Collections.Immutable;
using UI.Models;

public record AttractState
{
    public List<IAttractDetails> Videos { get; init; }

    public int CurrentAttractIndex { get; init; }

    public bool IsPlaying { get; init; }

    public bool IsPrimaryLanguageSelected { get; init; }

    public bool IsActive { get; init; }

    public int ConsecutiveAttractCount { get; init; }

    public int ModeTopperImageIndex { get; init; }

    public int ModeTopImageIndex { get; init; }

    public string? TopImageResourceKey { get; init; }

    public string? TopperImageResourceKey { get; init; }

    public bool IsAlternateTopImageActive { get; init; }

    public bool IsAlternateTopperImageActive { get; init; }

    public string? TopVideoPath { get; init; }

    public string? BottomVideoPath { get; init; }

    public bool IsTopperPlaying { get; init; }

    public bool IsTopPlaying { get; init; }

    public bool IsBottomPlaying { get; init; }

    public bool IsNextLanguagePrimary { get; init; }

    public bool IsLastInitialLanguagePrimary { get; init; }

    public bool ResetAttractOnInterruption { get; set; }

    public bool CanAttractModeStart { get; set; }

    public int AttractModeIdleTimeoutInSeconds { get; set; } = 30;
}
