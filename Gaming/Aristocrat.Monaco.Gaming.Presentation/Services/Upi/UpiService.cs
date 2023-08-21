namespace Aristocrat.Monaco.Gaming.Presentation.Services.Upi;

using Application.Contracts;
using Gaming.Contracts;
using Kernel;

public class UpiService : IUpiService
{
    private readonly IPropertiesManager _properties;

    public UpiService(IPropertiesManager properties)
    {
        _properties = properties;
    }

    public bool GetIsServiceAvailable() =>
        _properties.GetValue(GamingConstants.ShowServiceButton, false);

    public bool GetIsVolumeControlEnabled()
    {
        var volumeControlLocation = (VolumeControlLocation)_properties.GetValue(
            ApplicationConstants.VolumeControlLocationKey,
            ApplicationConstants.VolumeControlLocationDefault);

       return volumeControlLocation == VolumeControlLocation.Lobby ||
            volumeControlLocation == VolumeControlLocation.LobbyAndGame;
    }
}
