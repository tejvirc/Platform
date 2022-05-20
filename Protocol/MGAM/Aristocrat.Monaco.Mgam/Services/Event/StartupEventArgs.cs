namespace Aristocrat.Monaco.Mgam.Services.Event
{
    using System;
    using Kernel;

    /// <summary>
    ///     Startup event args.
    /// </summary>
    public class StartupEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StartupEventArgs"/> class.
        /// </summary>
        /// <param name="event"></param>
        public StartupEventArgs(IEvent @event)
        {
            Event = @event;
        }

        /// <summary>
        ///     Gets the event.
        /// </summary>
        public IEvent Event { get; }
    }
}
