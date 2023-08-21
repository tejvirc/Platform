namespace Aristocrat.G2S.Client
{
    using System;

    /// <summary>
    ///     Event args for a message related events
    /// </summary>
    public class MessageHandledEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageHandledEventArgs" /> class.
        /// </summary>
        /// <param name="command">The handled command</param>
        public MessageHandledEventArgs(ClassCommand command)
        {
            Command = command;
        }

        /// <summary>
        ///     The command that was handled
        /// </summary>
        public ClassCommand Command { get; }
    }
}