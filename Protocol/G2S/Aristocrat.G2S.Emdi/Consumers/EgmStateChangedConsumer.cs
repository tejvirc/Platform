namespace Aristocrat.G2S.Emdi.Consumers
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Client;
    using Client.Devices;
    using Events;
    using Extensions;
    using Host;
    using log4net;
    using Monaco.Application.Contracts.Localization;
    using Monaco.G2S.Common.Events;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Consumes the <see cref="EgmStateChangedEvent"/> event
    /// </summary>
    public class EgmStateChangedConsumer : Consumes<EgmStateChangedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly ILocalization _localization;
        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="EgmStateChangedConsumer"/> class.
        /// </summary>
        /// <param name="egm"></param>
        /// <param name="localization"></param>
        /// <param name="reporter"></param>
        public EgmStateChangedConsumer(
            IG2SEgm egm,
            ILocalization localization,
            IReporter reporter)
        {
            _egm = egm;
            _localization = localization;
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(EgmStateChangedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received EgmStateChangedEvent event");

                var device = _egm.GetDevice<ICabinetDevice>();

                await _reporter.ReportAsync(
                    new c_eventReportEventItem
                    {
                        eventCode = EventCodes.EgmStateChanged,
                        Item = new cabinetStatus
                        {
                            egmState = device.State.ToG2S(),
                            localeId = device.LocaleId(_localization.CurrentCulture)
                        }
                    },
                    EventCodes.EgmStateChanged);
            }
            catch (MessageException ex)
            {
                Logger.Error($"Error ({ex.ErrorCode}) sending EGM state changed event report", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending EGM state changed event report", ex);
            }
        }
    }
}
