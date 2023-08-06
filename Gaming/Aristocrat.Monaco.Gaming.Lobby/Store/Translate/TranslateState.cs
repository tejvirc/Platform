namespace Aristocrat.Monaco.Gaming.Lobby.Store.Translate;

using System.Collections.Immutable;
using Fluxor;

public record TranslateState
{
    public ImmutableList<string> LocaleCodes { get; init; }

    public bool IsMultiLangauge { get; init; }

    public bool IsPrimaryLanguageActive { get; init; }
}
