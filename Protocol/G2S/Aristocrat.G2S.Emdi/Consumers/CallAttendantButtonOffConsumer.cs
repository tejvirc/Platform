namespace Aristocrat.G2S.Emdi.Consumers
{
    using System;
    using System.Reflection;
    using Events;
    using Host;
    using Monaco.Gaming.Contracts;
    using Protocol.v21ext1b1;
    using System.Threading.Tasks;
    using log4net;

    /// <summary>
    /// Consumes the <see cref="CallAttendantButtonOffEvent"/> event.
    /// </summary>
    public class CallAttendantButtonOffConsumer : Consumes<CallAttendantButtonOffEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallAttendantButtonOffConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public CallAttendantButtonOffConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(CallAttendantButtonOffEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received CallAttendantButtonOffEvent event");

                await _reporter.ReportAsync(
                        new c_eventReportEventItem { eventCode = EventCodes.CallAttendantButtonPressed },
                        EventCodes.CallAttendantButtonPressed);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending change button pressed event report", ex);
            }
        }
    }
}
