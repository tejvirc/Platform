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
    /// Consume the <see cref="GameEndedEvent"/> event.
    /// </summary>
    public class GameEndedConsumer : Consumes<GameEndedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameEndedConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public GameEndedConsumer(
            IReporter reporter)

        {
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(GameEndedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received GameEndedEvent event");

                var events = await theEvent.CreateReportEventsAsync((evt, code) => code == EventCodes.G2SGameEnded ? theEvent.Log.ToRecallLog() : null,
                    EventCodes.GameEnded, EventCodes.G2SGameEnded);

                await _reporter.ReportAsync(events, EventCodes.GameEnded, EventCodes.G2SGameEnded);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending game end event report", ex);
            }
        }
    }
}
