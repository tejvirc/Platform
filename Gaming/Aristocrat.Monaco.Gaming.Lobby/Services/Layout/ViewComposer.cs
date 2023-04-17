namespace Aristocrat.Monaco.Gaming.Lobby.Services.Layout;

using System.Threading.Tasks;
using Regions;
using Views;

public class ViewComposer : IViewComposer
{
    private readonly IRegionManager _regionManager;
    private readonly LobbyConfiguration _configuration;

    public ViewComposer(IRegionManager regionManager, LobbyConfiguration configuration)
    {
        _regionManager = regionManager;
        _configuration = configuration;
    }

    public async Task ComposeAsync()
    {
        if (_configuration.MultiLanguageEnabled)
            await _regionManager.RegisterViewAsync<MultiLanguageUpi>(RegionNames.Upi, ViewNames.MultiLingualUpi);
        else
            await _regionManager.RegisterViewAsync<StandardUpi>(RegionNames.Upi, ViewNames.StandardUpi);
    }
}
