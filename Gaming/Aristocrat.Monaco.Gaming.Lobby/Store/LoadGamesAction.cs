namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using Models;

public class LoadGamesAction
{
    public LoadGamesAction(LoadGameTrigger trigger = LoadGameTrigger.OnDemand)
    {
        Trigger = trigger;
    }

    public LoadGameTrigger Trigger { get; }
}
