namespace Aristocrat.Mgam.Client.Routing
{
    using System;

    /// <summary>
    ///     Routed message event args.
    /// </summary>
    public class RoutedMessageEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoutedMessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">A routed message.</param>
        public RoutedMessageEventArgs(RoutedMessage message)
        {
            Message = message;
        }

        /// <summary>
        ///     Gets the routed message.
        /// </summary>
        public RoutedMessage Message { get; }
    }
}
