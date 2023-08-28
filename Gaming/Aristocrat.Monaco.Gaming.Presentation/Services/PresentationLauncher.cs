namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System;
using Gaming.Contracts.Lobby;
using Prism.Ioc;

// The ILobby service will eventually be removed for another service to boot up the
// presentation services for the platform. The lobby may not be owned by the platform
// in the future. However, the platform will continue to have overlays that need to
// display over the game and lobby windows.
public class PresentationLauncher : ILobby
{
    private readonly IPresentationService _presentaion;

    public PresentationLauncher(IPresentationService presentaion)
    {
        _presentaion = presentaion;
    }

    public void CreateWindow() =>
        _presentaion.StartAsync().Wait();

    public void Close() =>
        _presentaion.StopAsync().Wait();

    public void Show() => throw new NotSupportedException();

    public void Hide() => throw new NotSupportedException();
}
