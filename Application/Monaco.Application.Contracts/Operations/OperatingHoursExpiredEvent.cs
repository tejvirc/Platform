namespace Aristocrat.Monaco.Application.Contracts.Operations
{
    using System;
    using Kernel;

    /// <summary>
    ///     Event raised when the operating hours schedule indicates the cabinet should be disabled.
    /// </summary>
    [Serializable]
    public class OperatingHoursExpiredEvent : BaseEvent
    {
    }
}