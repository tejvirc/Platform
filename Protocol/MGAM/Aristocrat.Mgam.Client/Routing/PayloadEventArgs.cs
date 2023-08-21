namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using Messaging;

    /// <summary>
    ///     Payload event args.
    /// </summary>
    internal class PayloadEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PayloadEventArgs"/> class.
        /// </summary>
        /// <param name="payload"></param>
        public PayloadEventArgs(Payload payload)
        {
            Payload = payload;
        }

        /// <summary>
        ///     Gets the payload.
        /// </summary>
        public Payload Payload { get; }
    }
}
