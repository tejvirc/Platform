namespace Aristocrat.Monaco.Gaming.Presentation.Store.Game;

using System;
using Fluxor;

public class GameFeature : Feature<GameState>
{
    public override string GetName() => "Game";

    protected override GameState GetInitialState()
    {
        return new GameState
        {
            MainWindowHandle = IntPtr.Zero,
            TopWindowHandle = IntPtr.Zero,
            TopperWindowHandle = IntPtr.Zero,
            ButtonDeckWindowHandle = IntPtr.Zero
        };
    }
}
