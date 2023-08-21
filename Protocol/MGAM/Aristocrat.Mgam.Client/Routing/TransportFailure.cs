namespace Aristocrat.Mgam.Client.Routing
{
    /// <summary>
    ///     Defines a transport failure.
    /// </summary>
    public enum TransportFailure
    {
        /// <summary>No failure.</summary>
        None,

        /// <summary>A malformed message was received.</summary>
        Malformed,

        /// <summary>A response timeout occurred.</summary>
        Timeout,

        /// <summary>An invalid response was received from the server.</summary>
        InvalidServerResponse
    }
}
