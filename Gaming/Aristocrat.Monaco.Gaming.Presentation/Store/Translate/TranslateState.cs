namespace Aristocrat.Monaco.Gaming.Presentation.Store.Translate;

using System.Collections.Immutable;

public record TranslateState
{
    public ImmutableList<string> LocaleCodes { get; init; }

    public bool IsMultiLanguage { get; init; }

    public bool IsPrimaryLanguageActive { get; init; }
}