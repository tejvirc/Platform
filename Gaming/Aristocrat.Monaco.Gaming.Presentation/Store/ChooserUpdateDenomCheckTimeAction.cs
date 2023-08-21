namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System;

public record ChooserUpdateDenomCheckTimeAction
{
    public DateTime CheckTime { get; init; }
}
