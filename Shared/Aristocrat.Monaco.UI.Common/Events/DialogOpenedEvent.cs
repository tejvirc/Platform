namespace Aristocrat.Monaco.UI.Common.Events
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Definition of the DialogOpenedEvent class.
    ///     <remarks>
    ///         This event is posted by the DialogService
    ///         When a modal dialog window is opened.
    ///     </remarks>
    /// </summary>
    [ProtoContract]
    public class DialogOpenedEvent : BaseEvent
    {
    }
}