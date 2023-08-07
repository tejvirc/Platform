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
    /// Consumes the <see cref="CallAttendantButtonOnEvent"/> event.
    /// </summary>
    public class CallAttendantButtonOnConsumer : Consumes<CallAttendantButtonOnEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallAttendantButtonOnConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public CallAttendantButtonOnConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(CallAttendantButtonOnEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received CallAttendantButtonOnEvent event");

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
