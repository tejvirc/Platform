namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Posted when a provider is added to the <see cref="IMeterManager" />
    /// </summary>
    [ProtoContract]
    public class MeterProviderAddedEvent : BaseEvent
    {
    }
}