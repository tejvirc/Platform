namespace Aristocrat.Mgam.Client
{
    /// <summary>
    ///     The current state of the protocol client.
    /// </summary>
    public enum EgmState
    {
        /// <summary>The client has not been started.</summary>
        NotStarted,

        /// <summary>The client is starting.</summary>
        Starting,

        /// <summary>The client has been started.</summary>
        Started,

        /// <summary>The client is stopping.</summary>
        Stopping,

        /// <summary>The client has been stopped.</summary>
        Stopped,

        /// <summary>The client is in a failure state.</summary>
        Failure
    }
}
