namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event raised when the currency culture has been changed
    /// </summary>
    [ProtoContract]
    public class CurrencyCultureChangedEvent : BaseEvent
    {
    }
}