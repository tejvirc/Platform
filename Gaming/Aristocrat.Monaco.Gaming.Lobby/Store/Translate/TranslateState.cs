namespace Aristocrat.Monaco.Gaming.Lobby.Store.Translate;

using System.Collections.Immutable;
using Fluxor;

public record TranslateState
{
    public ImmutableList<string> LocalCodes { get; init; }

    public bool IsMultiLangaugeEnabled { get; init; }

    public bool IsPrimaryLanguageSelected { get; init; }
}
