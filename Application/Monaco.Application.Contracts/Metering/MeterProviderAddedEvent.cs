namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Posted when a provider is added to the <see cref="IMeterManager" />
    /// </summary>
    [Serializable]
    public class MeterProviderAddedEvent : BaseEvent
    {
    }
}