namespace Aristocrat.Monaco.Application.UI.Events
{
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Kernel;
    using System;

    /// <summary>
    ///     Definition of the OperatorMenuPageLoadedEvent class.
    ///     <remarks>
    ///         This event is posted when an Operator Menu page is loaded
    ///     </remarks>
    /// </summary>
    [Serializable]
    public class OperatorMenuPageLoadedEvent : BaseEvent
    {
        public OperatorMenuPageLoadedEvent(IOperatorMenuPageViewModel page)
        {
            Page = page;
        }

        public IOperatorMenuPageViewModel Page { get; }
    }
}