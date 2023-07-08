namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdatePrimaryLanguageIndicators
{
    public bool NextAttractModeLanguageIsPrimary { get; init; }

    public bool LastInitialAttractModeLanguageIsPrimary { get; init; }
}
