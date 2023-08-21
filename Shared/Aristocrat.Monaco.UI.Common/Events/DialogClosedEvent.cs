namespace Aristocrat.Monaco.UI.Common.Events
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Definition of the DialogClosedEvent class.
    ///     <remarks>
    ///         This event is posted by the DialogService
    ///         When a modal dialog window is closed.
    ///     </remarks>
    /// </summary>
    [ProtoContract]
    public class DialogClosedEvent : BaseEvent
    {
    }
}