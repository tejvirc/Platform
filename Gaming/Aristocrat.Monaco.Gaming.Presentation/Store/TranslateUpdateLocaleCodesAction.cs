namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System.Collections.Generic;
using System.Linq;

public record TranslateUpdateLocaleCodesAction
{
    public TranslateUpdateLocaleCodesAction(IEnumerable<string> localeCodes)
    {
        LocaleCodes = localeCodes.ToList();
    }

    public IReadOnlyList<string> LocaleCodes { get; }
}