namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Definition of the PersistentStorageIntegrityCheckFailedEvent class.
    /// </summary>
    [ProtoContract]
    public class PersistentStorageIntegrityCheckFailedEvent : BaseEvent
    {
    }
}