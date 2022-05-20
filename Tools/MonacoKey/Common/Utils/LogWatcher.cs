namespace Common.Utils
{
    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using System;
    using System.Text;

    // shoutout https://www.thepicketts.org/2012/12/how-to-watch-your-log-through-your-application-in-log4net/
    public class LogWatcher
    {
        private string logContent;

        private MemoryAppenderPlus memoryAppender;

        public event EventHandler Updated;

        public string LogContent
        {
            get { return logContent; }
        }

        public LogWatcher()
        {
            // Get the memory appender
            memoryAppender = (MemoryAppenderPlus)Array.Find(LogManager.GetRepository().GetAppenders(), GetMemoryAppender);

            // Read in the log content
            logContent = GetEvents(memoryAppender);

            // Add an event handler to handle updates from the MemoryAppender
            memoryAppender.Updated += HandleUpdate;
        }

        public void HandleUpdate(object sender, EventArgs e)
        {
            logContent += GetEvents(memoryAppender);

            // Then alert the Updated event that the LogWatcher has been updated
            var handler = Updated;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private static bool GetMemoryAppender(IAppender appender)
        {
            // Returns the IAppender named MemoryAppender in the Log4Net.config file
            if (appender.Name.Equals("MemoryAppender"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetEvents(MemoryAppenderPlus memoryAppender)
        {
            StringBuilder output = new StringBuilder();

            // Get any events that may have occurred
            LoggingEvent[] events = memoryAppender.GetEvents();

            // Check that there are events to return
            if (events != null && events.Length > 0)
            {
                // If there are events, we clear them from the logger, since we're done with them  
                memoryAppender.Clear();

                foreach (LoggingEvent ev in events)
                    output.Append(ev.TimeStamp.ToString("HH:mm:ss.f") + " " + ev.Level + ":      " + ev.RenderedMessage + "\r\n");
            }

            return output.ToString();
        }
    }
}
