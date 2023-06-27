namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Client;

using System;
using System.Diagnostics;
using Aristocrat.Monaco.Gaming.Contracts.Process;
using Aristocrat.Monaco.Gaming.Contracts;
using LobbyRuntime.V1;

public class LobbyClient
{
    private readonly LobbyServiceStub _lobbyStub = new();

    public void SendEvent(PlatformEvent evt, bool state)
    {
        Invoke(client => client.SendEvent(new SendNotificationRequest { EventCode = (NotificationCode)evt }));
    }

    private T? Invoke<T>(Func<LobbyServiceStub, T> callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        try
        {
            return callback(_lobbyStub);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return default;
    }
}
