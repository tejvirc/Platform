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
    /// Consumes the <see cref="SecondaryGameChoiceEvent"/> event.
    /// </summary>
    public class SecondaryGameChoiceConsumer : Consumes<SecondaryGameChoiceEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondaryGameChoiceConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public SecondaryGameChoiceConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(SecondaryGameChoiceEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received SecondaryGameChoiceEvent event");

                var events = await theEvent.CreateReportEventsAsync((evt, code) => code == EventCodes.G2SSecondaryGameChoice ? theEvent.Log.ToRecallLog() : null,
                        EventCodes.SecondaryGameChoice, EventCodes.G2SSecondaryGameChoice);

                await _reporter.ReportAsync(events, EventCodes.SecondaryGameChoice, EventCodes.G2SSecondaryGameChoice);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending secondary game choice event report.", ex);
            }
        }
    }
}
