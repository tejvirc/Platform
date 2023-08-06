namespace Aristocrat.Monaco.Gaming.Lobby.Store.Translate;

using System.Collections.Immutable;
using Fluxor;

public class TranslateFeature : Feature<TranslateState>
{
    public override string GetName() => "Translate";

    protected override TranslateState GetInitialState()
    {
        return new TranslateState
        {
            LocaleCodes = ImmutableList<string>.Empty,
            IsPrimaryLanguageActive = true
        };
    }
}
