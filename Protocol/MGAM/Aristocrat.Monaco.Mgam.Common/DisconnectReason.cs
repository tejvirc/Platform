namespace Aristocrat.Monaco.Mgam.Common
{
    /// <summary>
    ///     Defines reasons for disconnecting from the host.
    /// </summary>
    public enum DisconnectReason
    {
        /// <summary>Response was not received from the host in specified time.</summary>
        ResponseTimeout,

        /// <summary>Device configuration changed.</summary>
        DeviceChanged,

        /// <summary>Network configuration changed.</summary>
        NetworkChanged,

        /// <summary>Host configuration changed.</summary>
        HostChanged,

        /// <summary>Game configuration changed.</summary>
        GamesChanged,

        /// <summary>Invalid Server Response.</summary>
        InvalidServerResponse
    }
}
