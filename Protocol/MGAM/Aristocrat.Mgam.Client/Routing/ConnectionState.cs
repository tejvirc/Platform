namespace Aristocrat.Mgam.Client.Routing
{
    /// <summary>
    ///     End point state.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>No change in the connection state.</summary>
        Unchanged,

        /// <summary>End point is disconnected.</summary>
        Lost,

        /// <summary>End point is connected.</summary>
        Connected,

        /// <summary>End point is connected but has not been used for a duration of time.</summary>
        Idle
    }
}
