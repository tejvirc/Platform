namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify that the cabinet disk space is low in a hard tilt condition.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is posted when low disk space is monitored.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class DiskSpaceEvent : BaseEvent
    {
    }
}