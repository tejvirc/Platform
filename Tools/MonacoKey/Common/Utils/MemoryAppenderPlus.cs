namespace Common.Utils
{
    using log4net.Appender;
    using System;

    // Shoutout https://www.thepicketts.org/2012/12/how-to-watch-your-log-through-your-application-in-log4net/
    // This class allows me to use the log4net MemoryAppender, but view the logs on my UI.
    public class MemoryAppenderPlus : MemoryAppender
    {
        public event EventHandler Updated;

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            // Append the event as usual
            base.Append(loggingEvent);

            // Then alert the Updated event that an event has occurred
            var handler = Updated;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}
