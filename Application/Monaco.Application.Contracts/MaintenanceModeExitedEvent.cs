namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that maintenance mode has been exited.
    /// </summary>
    /// <remarks>
    ///     This event will be posted when maintenance mode is exited.
    ///     Typically caused by sending SAS LP0B.
    /// </remarks>
    [Serializable]
    public class MaintenanceModeExitedEvent : BaseEvent
    {
    }
}