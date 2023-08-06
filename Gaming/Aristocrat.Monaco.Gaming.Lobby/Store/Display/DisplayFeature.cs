namespace Aristocrat.Monaco.Gaming.Lobby.Store.Display;

using Fluxor;

internal class DisplayFeature : Feature<DisplayState>
{
    public override string GetName() => "Display";

    protected override DisplayState GetInitialState()
    {
        return new DisplayState
        {
            IsConnected = true
        };
    }
}
