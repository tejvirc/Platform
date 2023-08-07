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
    /// Consumes the <see cref="SecondaryGameStartedEvent"/> event
    /// </summary>
    public class SecondaryGameStartedConsumer : Consumes<SecondaryGameStartedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondaryGameStartedConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public SecondaryGameStartedConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(SecondaryGameStartedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received SecondaryGameStartedEvent event");

                var events = await theEvent.CreateReportEventsAsync((evt, code) => code == EventCodes.G2SSecondaryGameStarted ? theEvent.Log.ToRecallLog() : null,
                        EventCodes.SecondaryGameStarted, EventCodes.G2SSecondaryGameStarted);

                await _reporter.ReportAsync(events, EventCodes.SecondaryGameStarted, EventCodes.G2SSecondaryGameStarted);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending secondary game started event report", ex);
            }
        }
    }
}
