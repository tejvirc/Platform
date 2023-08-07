namespace Aristocrat.G2S.Emdi.Consumers
{
    using System;
    using System.Reflection;
    using Events;
    using Extensions;
    using Host;
    using Monaco.Gaming.Contracts;
    using System.Threading.Tasks;
    using log4net;

    /// <summary>
    /// Consumes the <see cref="PrimaryGameStartedEvent"/> event.
    /// </summary>
    public class PrimaryGameStartedConsumer : Consumes<PrimaryGameStartedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryGameStartedConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public PrimaryGameStartedConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(PrimaryGameStartedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received PrimaryGameStartedEvent event");

                var events = await theEvent.CreateReportEventsAsync((evt, code) => code == EventCodes.G2SPrimaryGameStarted ? theEvent.Log.ToRecallLog() : null,
                        EventCodes.PrimaryGameStarted, EventCodes.G2SPrimaryGameStarted);

                await _reporter.ReportAsync(events, EventCodes.PrimaryGameStarted, EventCodes.G2SPrimaryGameStarted);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending primary game started event report", ex);
            }
        }
    }
}
