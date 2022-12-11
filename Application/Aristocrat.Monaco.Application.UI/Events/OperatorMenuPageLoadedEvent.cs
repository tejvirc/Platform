namespace Aristocrat.Monaco.Application.UI.Events
{
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Kernel;
    using ProtoBuf;
    using System;

    /// <summary>
    ///     Definition of the OperatorMenuPageLoadedEvent class.
    ///     <remarks>
    ///         This event is posted when an Operator Menu page is loaded
    ///     </remarks>
    /// </summary>
    [ProtoContract]
    public class OperatorMenuPageLoadedEvent : BaseEvent
    {
        public OperatorMenuPageLoadedEvent(IOperatorMenuPageViewModel page)
        {
            Page = page;
        }

        [ProtoMember(1)]
        public IOperatorMenuPageViewModel Page { get; }
    }
}