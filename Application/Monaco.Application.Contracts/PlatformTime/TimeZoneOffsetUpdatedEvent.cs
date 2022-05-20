namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the system time zone offset has been updated.
    /// </summary>
    /// <remarks>
    ///     This event will be posted when the system time zone offset is updated successfully
    ///     through the <c>TimeZoneInformation</c> of <c>ITime</c>. Any component which is sensitive to
    ///     the system time zone adjustment should consider handling this event.
    /// </remarks>
    [Serializable]
    public class TimeZoneOffsetUpdatedEvent : BaseEvent
    {
    }
}