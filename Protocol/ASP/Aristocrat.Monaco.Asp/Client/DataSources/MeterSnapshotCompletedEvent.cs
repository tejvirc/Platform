namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     A MeterSnapshotCompletedEvent should be posted when the MeterSnapshot is updated.
    /// </summary>
    [ProtoContract]
    public class MeterSnapshotCompletedEvent : BaseEvent
    {
    }
}