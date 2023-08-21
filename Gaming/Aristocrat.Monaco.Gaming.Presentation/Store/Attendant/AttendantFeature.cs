﻿namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attendant;

using Fluxor;

public class AttendantFeature : Feature<AttendantState>
{
    public override string GetName() => "Attendant";

    protected override AttendantState GetInitialState()
    {
        return new AttendantState();
    }
}
