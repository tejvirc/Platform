namespace Aristocrat.Monaco.Application.Contracts.SerialGat
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to disseminate serial GAT status message
    /// </summary>
    [Serializable]
    public class SerialGatStatusEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="statusMessage">Status message</param>
        public SerialGatStatusEvent(string statusMessage)
        {
            StatusMessage = statusMessage;
        }

        /// <summary>
        ///     Status message text
        /// </summary>
        public string StatusMessage { get; }
    }
}