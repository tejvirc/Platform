namespace Aristocrat.Monaco.Gaming.Presentation.Services.Game;

using Fluxor;

public class GameService : IGameService
{
    private readonly IStore _store;

    public GameService(IStore store)
    {
        _store = store;
    }

    public void OnGameEnabled()
    {
        if (!_store.Initialized.IsCompletedSuccessfully)
        {
            return;
        }
    }
}
