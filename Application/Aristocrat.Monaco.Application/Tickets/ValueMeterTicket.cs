namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Creates a text ticket displaying value meters
    /// </summary>
    public class ValueMeterTicket : AuditTicket
    {
        private readonly bool _useMasterValues;
        private readonly IList<Tuple<IMeter, string>> _meters;

        public ValueMeterTicket(string titleOverride, bool useMasterValues, IList<Tuple<IMeter, string>> meters)
            : base(titleOverride)
        {
            _useMasterValues = useMasterValues;
            _meters = meters;
        }

        /// <inheritdoc />
        public override int TicketHeaderLineCount => 8;

        protected bool UseMasterValues => _useMasterValues;

        protected IList<Tuple<IMeter, string>> Meters => _meters;

        /// <inheritdoc />
        public override void AddTicketHeader()
        {
            AddEmptyLines(2);

            AddLine(null, (string)PropertiesManager.GetProperty(PropertyKey.TicketTextLine1, string.Empty), null);
            AddLine(null, (string)PropertiesManager.GetProperty(ApplicationConstants.Zone,
                    TicketLocalizer.GetString(ResourceKeys.DataUnavailable)), null);
            AddLine(null, PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey,
                    TicketLocalizer.GetString(ResourceKeys.DataUnavailable)).ToUpper(), null);
            var serialNumber = PropertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            AddLine(null, string.Format(CultureInfo.CurrentCulture, "{0}: {1}",
                    TicketLocalizer.GetString(ResourceKeys.SerialNumberText).ToUpper(), serialNumber), null);
            AddLine(null, (string)PropertiesManager.GetProperty(KernelConstants.SystemVersion,
                    TicketLocalizer.GetString(ResourceKeys.NotSet)), null);

            AddEmptyLines(4);

            var dateTimeNow = ServiceManager.GetService<ITime>().GetLocationTime(DateTime.UtcNow);
            var dateText = TicketLocalizer.GetString(ResourceKeys.DateText).ToUpper() + ": {0}";
            var timeText = TicketLocalizer.GetString(ResourceKeys.TimeText).ToUpper() + ": {0}";
            AddLine(null, string.Format(CultureInfo.CurrentCulture, dateText, dateTimeNow.ToString(DateFormat)), null);
            AddLine(null, string.Format(CultureInfo.CurrentCulture, timeText, dateTimeNow.ToString(TimeFormat)), null);

            AddEmptyLines();
        }

        /// <inheritdoc />
        public override void AddTicketContent()
        {
            AddTicketContentHeader();

            foreach (var m in Meters)
            {
                var meter = m.Item1;
                var meterValue = UseMasterValues ? meter.Lifetime : meter.Period;

                var valueString = meter.Classification.CreateValueString(meterValue, TicketLocalizer.CurrentCulture);
                valueString = Regex.Replace(valueString, @"[^\u0000-\u007F]+", " ");

                AddLine(m.Item2, null, valueString);
            }
        }

        protected virtual void AddTicketContentHeader()
        {
            var meterManager = ServiceManager.GetService<IMeterManager>();

            var leftHeader = TicketLocalizer.GetString(ResourceKeys.Meter);
            var rightHeader = TicketLocalizer.GetString(ResourceKeys.Value);

            string meterHeader;
            string lastResetDate;
            string lastResetTime;
            if (UseMasterValues)
            {
                meterHeader = TicketLocalizer.GetString(ResourceKeys.MasterMetersTicketMetersHeaderText).ToUpper();
                var clearDateTime = Time.GetLocationTime(meterManager.LastMasterClear);
                lastResetDate = clearDateTime.ToString(DateFormat);
                lastResetTime = clearDateTime.ToString(TimeFormat);
            }
            else
            {
                meterHeader = TicketLocalizer.GetString(ResourceKeys.PeriodMetersTicketMetersHeaderText).ToUpper();
                var clearDateTime = Time.GetLocationTime(meterManager.LastPeriodClear);
                lastResetDate = clearDateTime.ToString(DateFormat);
                lastResetTime = clearDateTime.ToString(TimeFormat);
            }

            AddLine(leftHeader, null, rightHeader);

            AddDashesLine();

            AddLine(null, meterHeader, null);
            var lastResetText = TicketLocalizer.GetString(ResourceKeys.LastResetText).ToUpper() + ": {0}  {1}";
            AddLine(null, string.Format(CultureInfo.CurrentCulture, lastResetText, lastResetDate, lastResetTime), null);

            AddEmptyLines();
        }
    }
}
