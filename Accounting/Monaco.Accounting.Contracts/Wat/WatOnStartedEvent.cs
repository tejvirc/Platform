namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Event emitted when a Wat transfer has started.
    /// </summary>
    /// <remarks>
    ///     This event is posted when the transfer on request is received from the transfer host
    ///     (or the IWatTransferOnProvider). This signals the start of the transfer. This can be used by the
    ///     client to perform any messaging to the user, but otherwise the client should not allow any other
    ///     actions to take place.
    /// </remarks>
    [Serializable]
    public class WatOnStartedEvent : BaseEvent
    {
    }
}