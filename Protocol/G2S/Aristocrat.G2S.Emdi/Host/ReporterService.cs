namespace Aristocrat.G2S.Emdi.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Events;
    using log4net;
    using Meters;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Implements the <see cref="IReporter"/> interface
    /// </summary>
    public class ReporterService : IReporter
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventSubscriptions _eventSubscriptions;
        private readonly IMeterSubscriptions _meterSubscriptions;
        private readonly IHostQueue _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReporterService"/> class.
        /// </summary>
        /// <param name="eventSubscriptions"></param>
        /// <param name="meterSubscriptions"></param>
        /// <param name="server"></param>
        public ReporterService(
            IEventSubscriptions eventSubscriptions,
            IMeterSubscriptions meterSubscriptions,
            IHostQueue server)
        {
            _eventSubscriptions = eventSubscriptions;
            _meterSubscriptions = meterSubscriptions;
            _server = server;
        }

        /// <inheritdoc />
        public async Task ReportAsync(c_eventReportEventItem @event, params string[] eventCodes)
        {
            await ReportAsync(new[] { @event }, eventCodes);
        }

        /// <inheritdoc />
        public async Task ReportAsync(IEnumerable<c_eventReportEventItem> events, params string[] eventCodes)
        {
            var subscribers = await _eventSubscriptions.GetSubscribersAsync(eventCodes);

            var items = events.ToArray();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    Logger.Debug($"EMDI: Sending events ({string.Join(",", items.Select(e => e.eventCode.ToString()))}) to port {subscriber.Port}");

                    await _server[subscriber.Port].SendCommandAsync<mdEventHandler, eventReport, eventAck>(
                        new eventReport { eventItem = items });
                }
                catch (MessageException ex)
                {
                    Logger.Error($"EMDI: Error ({ex.ErrorCode}) sending event to {subscriber.Port}: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Logger.Error($"EMDI: Error sending event to port {subscriber.Port}: {ex.Message}", ex);
                }
            }
        }

        /// <inheritdoc />
        public async Task ReportAsync(c_meterInfo meter, params string[] meterNames)
        {
            await ReportAsync(new[] { meter }, meterNames);
        }

        /// <inheritdoc />
        public async Task ReportAsync(IEnumerable<c_meterInfo> meters, params string[] meterNames)
        {
            var subscribers = await _meterSubscriptions.GetSubscribersAsync(meterNames);

            var items = meters.ToArray();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    await _server[subscriber.Port].SendCommandAsync<mdMeters, meterReport, meterReportAck>(
                        new meterReport { meterInfo = items });
                }
                catch (MessageException ex)
                {
                    Logger.Error($"EMDI: Error ({ex.ErrorCode}) sending meter to {subscriber.Port}: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    Logger.Error($"EMDI: Error sending meter to port {subscriber.Port}: {ex.Message}", ex);
                }
            }
        }
    }
}
