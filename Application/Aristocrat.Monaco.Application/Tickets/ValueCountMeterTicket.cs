namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Contracts;
    using Contracts.Extensions;
    using Kernel;

    /// <summary>
    ///     Creates a text ticket displaying value/count meters
    /// </summary>
    public class ValueCountMeterTicket : ValueMeterTicket
    {
        private readonly IList<Tuple<Tuple<IMeter, IMeter>, string>> _meters;

        public ValueCountMeterTicket(string titleOverride, bool useMasterValues, IList<Tuple<Tuple<IMeter, IMeter>, string>> meters)
            : base(titleOverride, useMasterValues, null)
        {
            _meters = meters;
        }

        public override void AddTicketContent()
        {
            AddTicketContentHeader();

            var multiplier = PropertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier);

            foreach (var meter in _meters)
            {
                if (meter.Item1.Item1 != null)
                {
                    var meterValue = UseMasterValues ? meter.Item1.Item1.Lifetime : meter.Item1.Item1.Period;
                    // the count (occurrence)
                    var meterNameString = $"{meter.Item2} Count";

                    var occurrenceValueString = meter.Item1.Item1.Classification.CreateValueString(meterValue);
                    occurrenceValueString = Regex.Replace(occurrenceValueString, @"[^\u0000-\u007F]+", " ");

                    AddLine(meterNameString, null, occurrenceValueString);
                }

                if (meter.Item1.Item2 != null)
                {
                    // add the meter value (not count)
                    var meterNameString = $"{meter.Item2} Value";

                    var meterValue = UseMasterValues ? meter.Item1.Item2.Lifetime : meter.Item1.Item2.Period;
                    var valueString = Convert.ToDouble(meterValue / multiplier).ToString(CultureInfo.CurrentCulture).FormattedCurrencyString();

                    AddLine(meterNameString, null, valueString);
                }
            }
        }
    }
}
