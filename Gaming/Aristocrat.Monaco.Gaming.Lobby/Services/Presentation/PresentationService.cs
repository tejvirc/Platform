namespace Aristocrat.Monaco.Gaming.UI.Presentation;

using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Lobby.Views;
using Aristocrat.Monaco.UI.Common;
using Common;
using Lobby.Services;
using Microsoft.Extensions.Logging;

public class PresentationService : IPresentation, Contracts.Lobby.ILobby
{
    private const string ShellWindowName = "Shell";
    private const string StatusWindowName = "StatusWindow";

    private readonly ILogger<PresentationService> _logger;
    private readonly ILobby _lobby;
    private readonly IWpfWindowLauncher _windowLauncher;

    public PresentationService(ILogger<PresentationService> logger, ILobby lobby, IWpfWindowLauncher windowLauncher)
    {
        _logger = logger;
        _lobby = lobby;
        _windowLauncher = windowLauncher;
    }

    public void CreateWindow()
    {
        _windowLauncher.Hide(StatusWindowName);
        _windowLauncher.CreateWindow<Shell>(ShellWindowName);

        Task.Run(async () => await _lobby.StartAsync())
            .FireAndForget(ex => _logger.LogError(ex, "Failure starting the lobby"));
    }

    public void Close()
    {
        Task.Run(async () => await _lobby.StopAsync())
            .FireAndForget(ex => _logger.LogError(ex, "Failure stopping the lobby"));

        _windowLauncher.Close(ShellWindowName);
        _windowLauncher.Show(StatusWindowName);
    }

    public void Hide()
    {
        throw new System.NotImplementedException();
    }

    public void Show()
    {
        throw new System.NotImplementedException();
    }
}
