namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record TranslateUpdatePrimaryLanguageIndicators
{
    public bool NextAttractModeLanguageIsPrimary { get; init; }

    public bool LastInitialAttractModeLanguageIsPrimary { get; init; }
}
