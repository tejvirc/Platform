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
    /// Consumes the <see cref="GameIdleEvent"/> event.
    /// </summary>
    public class GameIdleConsumer : Consumes<GameIdleEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameIdleConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public GameIdleConsumer(
            IReporter reporter)
        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(GameIdleEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received GameIdleEvent event");

                var events = await theEvent.CreateReportEventsAsync((evt, code) => code == EventCodes.G2SGameIdle ? theEvent.Log.ToRecallLog() : null,
                    EventCodes.GameIdle, EventCodes.G2SGameIdle);

                await _reporter.ReportAsync(events, EventCodes.GameIdle, EventCodes.G2SGameIdle);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending game idle event report", ex);
            }
        }
    }
}
