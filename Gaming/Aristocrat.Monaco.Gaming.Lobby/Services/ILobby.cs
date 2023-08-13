namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System.Threading.Tasks;

public interface ILobby
{
    public Task StartAsync();

    public Task StopAsync();
}
