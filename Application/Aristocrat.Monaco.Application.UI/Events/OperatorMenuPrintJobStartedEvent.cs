namespace Aristocrat.Monaco.Application.UI.Events
{
    using Kernel;
    using System;

    /// <summary>
    ///     Definition of the OperatorMenuPrintJobStartedEvent class.
    ///     <remarks>
    ///         This event is posted by the OperatorMenuPrintHandler
    ///         When a print job (possibly multiple tickets) has started.
    ///         It can be posted externally to control/disable the print 
    ///         buttons in the operator menu
    ///     </remarks>
    /// </summary>
    [Serializable]
    public class OperatorMenuPrintJobStartedEvent : BaseEvent
    {
    }
}