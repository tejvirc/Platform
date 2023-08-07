namespace Aristocrat.G2S.Emdi.Consumers
{
    using System;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Client;
    using Client.Devices;
    using Events;
    using Extensions;
    using Host;
    using log4net;
    using Monaco.Gaming.Contracts;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Consumes the <see cref="PlayerLanguageChangedEvent"/> event
    /// </summary>
    public class PlayerLanguageChangedConsumer : Consumes<PlayerLanguageChangedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLanguageChangedConsumer"/> class.
        /// </summary>
        /// <param name="egm"></param>
        /// <param name="reporter">IEmdiReporter instance</param>
        public PlayerLanguageChangedConsumer(
            IG2SEgm egm,
            IReporter reporter)
        {
            _egm = egm;
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(PlayerLanguageChangedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received PlayerLanguageChangedEvent event");

                var device = _egm.GetDevice<ICabinetDevice>();
                if (device == null)
                {
                    throw new Exception("Missing cabinet device, skipping report event");
                }

                await _reporter.ReportAsync(
                    new c_eventReportEventItem
                    {
                        eventCode = EventCodes.LocaleChanged,
                        Item = new cabinetStatus
                        {
                            egmState = device.State.ToG2S(),
                            localeId = LocaleId(theEvent.LocaleCode)
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

        private string LocaleId(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                return localeCode;
            }

            var match = Regex.Match(localeCode, @"(?<language>\w{2})([-_](?<region>\w{2}))?", RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return localeCode;
            }

            var language = match.Groups["language"].Value;
            var region = match.Groups["region"].Value;

            var result = language.ToLower();

            if (!string.IsNullOrEmpty(region))
            {
                result += $"_{region.ToUpper()}";
            }

            return result;
        }
    }
}
