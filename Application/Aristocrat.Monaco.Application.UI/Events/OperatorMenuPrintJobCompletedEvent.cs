namespace Aristocrat.Monaco.Application.UI.Events
{
    using Kernel;
    using ProtoBuf;
    using System;

    /// <summary>
    ///     Definition of the OperatorMenuPrintJobCompletedEvent class.
    ///     <remarks>
    ///         This event is posted by the OperatorMenuPrintHandler
    ///         When a print job (possibly multiple tickets) is completed.
    ///     </remarks>
    /// </summary>
    [ProtoContract]
    public class OperatorMenuPrintJobCompletedEvent : BaseEvent
    {
    }
}