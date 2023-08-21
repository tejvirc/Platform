namespace Aristocrat.Monaco.Gaming.Presentation.Store.PlayerMenu;

using Fluxor;

public class PlayerMenuFeature : Feature<PlayerMenuState>
{
    public override string GetName() => "PlayerMenu";

    protected override PlayerMenuState GetInitialState()
    {
        return new PlayerMenuState();
    }
}
