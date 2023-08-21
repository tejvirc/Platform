namespace Aristocrat.Monaco.Gaming.Presentation.Store.PlayerInfo;

using Fluxor;

public class PlayerInfoFeature : Feature<PlayerInfoState>
{
    public override string GetName() => "PlayerInfo";

    protected override PlayerInfoState GetInitialState()
    {
        return new PlayerInfoState();
    }
}
