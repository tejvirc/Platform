namespace Aristocrat.Monaco.Application.UI.Events
{
    using System;
    using Kernel;

    [Serializable]
    public class OperatorMenuWarningMessageEvent : BaseEvent
    {
        public OperatorMenuWarningMessageEvent(string message = null)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
