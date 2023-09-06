﻿namespace Aristocrat.Monaco.Gaming.Presentation.Store;

public record ChooserUpdateDenomFilterAction
{
    public ChooserUpdateDenomFilterAction(int filter)
    {
        Filter = filter;
    }

    public int Filter { get; }
}
