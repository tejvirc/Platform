namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.Tickets;
    using Contracts;
    using Contracts.Meters;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;

    /// <summary>
    ///     This class creates audit slip ticket objects
    /// </summary>
    public class AuditTicketCreator : IAuditTicketCreator, IService, IDisposable
    {
        // Ticket type
        private const string TicketType = "auditText";

        // Date and time format strings
        private static readonly DateTimeFormatInfo DateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        private static readonly string DateFormat = DateTimeFormat.ShortDatePattern;
        private static readonly string TimeFormat = ApplicationConstants.DefaultTimeFormat;

        // The persisted field name for audit slip number
        private const string AuditSlipNumberFieldName = "AuditSlipNumber";

        // The number of audit slip tickets printed
        private int _auditSlipNumber;

        private ServiceWaiter _serviceWaiter;

        private bool _disposed;

        /// <summary>
        ///     Creates an audit slip ticket.
        /// </summary>
        /// <param name="door">The door that was accessed</param>
        /// <returns>A Ticket object with fields required for an audit slip ticket.</returns>
        public Ticket CreateAuditTicket(string door)
        {
            var serviceManager = ServiceManager.GetInstance();
            var propertiesManager = serviceManager.GetService<IPropertiesManager>();
            var meterManager = serviceManager.GetService<IMeterManager>();
            var time = serviceManager.GetService<ITime>();
            door = door.ToUpper();

            var ticket = new Ticket
            {
                ["ticket type"] = TicketType,
                ["title"] = string.Format(CultureInfo.CurrentCulture, Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AuditSlipPeriodMetersText), door)
            };

            var leftText = new StringBuilder(FormatLineFeeds(10));
            leftText.Append(TicketConstants.Dashes + FormatLineFeeds(4));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DollarsInText), FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DollarsOutText), FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DollarsPlayedText), FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DollarsWonText), FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NetText), FormatLineFeeds(1));
            leftText.Append(TicketConstants.Dashes + FormatLineFeeds(4));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DollarsInText), FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DollarsOutText), FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DollarsPlayedText), FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DollarsWonText), FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NetText), FormatLineFeeds(1));
            leftText.Append(TicketConstants.Dashes + FormatLineFeeds(1));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AuditSlipCurrentCreditsText), FormatLineFeeds(1));
            var leftAlt = new StringBuilder(leftText + Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AuditSlipCurrentCreditWagerText) + FormatLineFeeds(3));
            leftText.AppendFormat("{0}:{1}", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AuditSlipCurrentWagerText), FormatLineFeeds(3));
            var auditSlipNumberText = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AuditSlipNumberText) + ": {0}";
            UpdateFieldsBuilder(
                leftText,
                leftAlt,
                string.Format(CultureInfo.CurrentCulture, auditSlipNumberText, _auditSlipNumber) +
                FormatLineFeeds(2));
            leftAlt.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SupplierShortText));
            leftText.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SupplierText));

            ticket["left"] = leftText.ToString();
            ticket["left alt"] = leftAlt.ToString();

            var centerText = new StringBuilder(FormatLineFeeds(1));
            centerText.Append(
                propertiesManager.GetValue(PropertyKey.TicketTextLine1, string.Empty) +
                FormatLineFeeds(1));
            var visible = ConfigWizardUtil.VisibleByConfig(propertiesManager, ApplicationConstants.ConfigWizardIdentityPageZoneOverride);
            centerText.Append(
                propertiesManager.GetValue(visible ? ApplicationConstants.Zone : "", Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DataUnavailable)) +
                FormatLineFeeds(1));
            var centerAlt = new StringBuilder(
                centerText +
                propertiesManager.GetValue(ApplicationConstants.JurisdictionKey, Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotSet)).ToUpper() +
                " " +
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.VideoLotteryText) +
                FormatLineFeeds(1));

            centerText.Append(
                propertiesManager.GetValue(ApplicationConstants.JurisdictionKey, Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotSet)) +
                FormatLineFeeds(1));

            var serialNumber = propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);

            UpdateFieldsBuilder(
                centerText,
                centerAlt,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "{0}: {1}",
                    Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SerialNumberText),
                    serialNumber) + FormatLineFeeds(1));

            centerAlt.Append(
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SoftwareVersionText) + ": ");

            UpdateFieldsBuilder(
                centerText,
                centerAlt,
                propertiesManager.GetValue(KernelConstants.SystemVersion, Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotSet)) +
                FormatLineFeeds(3));

            var masterClearDateTime = time.GetLocationTime(meterManager.LastMasterClear);
            var dateFormat = masterClearDateTime.ToString(DateFormat);
            var timeFormat = masterClearDateTime.ToString(TimeFormat);
            var dateTimeNow = time.GetLocationTime(DateTime.UtcNow);
            var dateText = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.DateText);
            var timeText = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.TimeText) + ": {0}";
            var lastResetText = Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.LastResetText) + ": {0}  {1}";
            var periodClearDateTime = time.GetLocationTime(meterManager.LastPeriodClear);
            var periodDateFormat = periodClearDateTime.ToString(DateFormat);
            var periodTimeFormat = periodClearDateTime.ToString(TimeFormat);

            var temp = new StringBuilder(
                string.Format(CultureInfo.CurrentCulture, dateText, dateTimeNow.ToString(DateFormat)) +
                FormatLineFeeds(1));
            temp.Append(
                string.Format(CultureInfo.CurrentCulture, timeText, dateTimeNow.ToString(TimeFormat)) +
                FormatLineFeeds(2));
            temp.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AuditSlipMasterMetersText) + FormatLineFeeds(1));
            temp.Append(
                string.Format(CultureInfo.CurrentCulture, lastResetText, dateFormat, timeFormat) +
                FormatLineFeeds(8));
            temp.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AuditSlipPeriodMetersText) + FormatLineFeeds(1));
            temp.Append(string.Format(CultureInfo.CurrentCulture, lastResetText, periodDateFormat, periodTimeFormat));
            UpdateFieldsBuilder(
                centerText,
                centerAlt,
                temp.ToString());

            ticket["center"] = centerText.ToString();
            ticket["center alt"] = centerAlt.ToString();

            var rightText = new StringBuilder(FormatLineFeeds(14));
            long meterMasterNet = 0;
            long meterPeriodNet = 0;

            var tempPeriodString = new StringBuilder(FormatLineFeeds(4));
            IMeter meter;
            IMeter netMeter = null;
            if (meterManager.IsMeterProvided(ApplicationMeters.TotalIn))
            {
                meter = meterManager.GetMeter(ApplicationMeters.TotalIn);
                netMeter = meter;
                var valueString = meter.Classification.CreateValueString(meter.Lifetime);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                rightText.Append(valueString + FormatLineFeeds(1));
                valueString = meter.Classification.CreateValueString(meter.Period);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                tempPeriodString.Append(valueString + FormatLineFeeds(1));
                meterMasterNet = meter.Lifetime;
                meterPeriodNet = meter.Period;
            }
            else
            {
                rightText.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
                tempPeriodString.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
            }

            if (meterManager.IsMeterProvided(ApplicationMeters.TotalOut))
            {
                meter = meterManager.GetMeter(ApplicationMeters.TotalOut);
                netMeter = meter;
                var valueString = meter.Classification.CreateValueString(meter.Lifetime);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                rightText.Append(valueString + FormatLineFeeds(1));
                valueString = meter.Classification.CreateValueString(meter.Period);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                tempPeriodString.Append(valueString + FormatLineFeeds(1));
                meterMasterNet -= meter.Lifetime;
                meterPeriodNet -= meter.Period;
            }
            else
            {
                rightText.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
                tempPeriodString.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
            }

            if (meterManager.IsMeterProvided(GamingMeters.WageredAmount))
            {
                meter = meterManager.GetMeter(GamingMeters.WageredAmount);
                netMeter = meter;
                var valueString = meter.Classification.CreateValueString(meter.Lifetime);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                rightText.Append(valueString + FormatLineFeeds(1));
                valueString = meter.Classification.CreateValueString(meter.Period);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                tempPeriodString.Append(valueString + FormatLineFeeds(1));
            }
            else
            {
                rightText.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
                tempPeriodString.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
            }

            if (meterManager.IsMeterProvided(GamingMeters.TotalEgmPaidGameWonAmount))
            {
                meter = meterManager.GetMeter(GamingMeters.TotalEgmPaidGameWonAmount);
                netMeter = meter;
                var valueString = meter.Classification.CreateValueString(meter.Lifetime);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                rightText.Append(valueString + FormatLineFeeds(1));
                valueString = meter.Classification.CreateValueString(meter.Period);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                tempPeriodString.Append(valueString + FormatLineFeeds(1));
            }
            else
            {
                rightText.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
                tempPeriodString.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
            }

            if (netMeter != null)
            {
                var valueString = netMeter.Classification.CreateValueString(meterMasterNet);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                rightText.Append(valueString + FormatLineFeeds(1));
                valueString = netMeter.Classification.CreateValueString(meterPeriodNet);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                tempPeriodString.Append(valueString + FormatLineFeeds(1));
            }
            else
            {
                rightText.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
                tempPeriodString.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText) + FormatLineFeeds(1));
            }

            rightText.Append(tempPeriodString + FormatLineFeeds(1));

            var balance = propertiesManager.GetValue(PropertyKey.CurrentBalance, 0L);
            var multiplier = propertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 0d);
            var valueStrings = (balance / (decimal)multiplier).FormattedCurrencyString();
            valueStrings = Regex.Replace(valueStrings, @"[^\u0000-\u007F]+", " ");
            rightText.Append(valueStrings + FormatLineFeeds(1));

            var gameState = serviceManager.GetService<IGamePlayState>();
            var gameHistory = serviceManager.GetService<IGameHistory>();

            if ((gameState.InGameRound || gameHistory.IsRecoveryNeeded) && gameHistory?.CurrentLog != null)
            {
                var currentWagerMeter = (gameHistory.CurrentLog.FinalWager * GamingConstants.Millicents) / multiplier;
                var valueString = currentWagerMeter.FormattedCurrencyString();
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");
                rightText.Append(valueString + FormatLineFeeds(1));
            }
            else
            {
                rightText.Append(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NotProvidedText));
            }

            ticket["right"] = rightText.ToString();

            _auditSlipNumber++;
            var storageService = serviceManager.GetService<IPersistentStorageManager>();
            var blockName = GetType().ToString();
            var block = storageService.GetBlock(blockName);
            block[AuditSlipNumberFieldName] = _auditSlipNumber;

            return ticket;
        }

        /// <inheritdoc />
        public string Name => "Audit Ticket Creator";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAuditTicketCreator) };

        /// <inheritdoc />
        public void Initialize()
        {
            var serviceManager = ServiceManager.GetInstance();

            var storageService = serviceManager.GetService<IPersistentStorageManager>();
            var blockName = GetType().ToString();

            if (storageService.BlockExists(blockName))
            {
                var block = storageService.GetBlock(blockName);
                _auditSlipNumber = (int)block[AuditSlipNumberFieldName];
            }
            else
            {
                var block = storageService.CreateBlock(PersistenceLevel.Critical, blockName, 1);
                _auditSlipNumber = 1;
                using (var transaction = block.StartTransaction())
                {
                    transaction[AuditSlipNumberFieldName] = _auditSlipNumber;
                    transaction.Commit();
                }
            }

            Task.Run(
                () =>
                {
                    var eventBus = serviceManager.GetService<IEventBus>();
                    _serviceWaiter = new ServiceWaiter(eventBus);
                    _serviceWaiter.AddServiceToWaitFor<IGameMeterManager>();
                    _serviceWaiter.AddServiceToWaitFor<IGamePlayState>();
                    _serviceWaiter.AddServiceToWaitFor<IGameHistory>();

                    _serviceWaiter.WaitForServices();

                    eventBus.Publish(new AuditTicketCreatorInitializedEvent());
                });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_serviceWaiter != null)
                {
                    _serviceWaiter.Dispose();
                }
            }

            _serviceWaiter = null;

            _disposed = true;
        }

        private static void UpdateFieldsBuilder(StringBuilder field, StringBuilder fieldAlt, string property)
        {
            field.Append(property);
            fieldAlt.Append(property);
        }

        private static string FormatLineFeeds(int count)
        {
            var lineFeeds = string.Empty;
            for (var i = 0; i < count; i++)
            {
                lineFeeds += "\n\r";
            }

            return lineFeeds;
        }
    }
}