namespace Aristocrat.Monaco.Sas
{
    using System.Collections.Generic;
    using Accounting.Contracts;

    /// <summary>
    ///     Converts a denomination to a meter name
    /// </summary>
    public static class DenominationToMeterName
    {
        private static readonly Dictionary<long, string> MeterNameMap = new Dictionary<long, string>
        {
            { 1_00, AccountingMeters.BillCount1s },
            { 2_00, AccountingMeters.BillCount2s },
            { 5_00, AccountingMeters.BillCount5s },
            { 10_00, AccountingMeters.BillCount10s },
            { 20_00, AccountingMeters.BillCount20s },
            { 25_00, AccountingMeters.BillCount25s },
            { 50_00, AccountingMeters.BillCount50s },
            { 100_00, AccountingMeters.BillCount100s },
            { 200_00, AccountingMeters.BillCount200s },
            { 250_00, "BillCount250s" }, //not defined in AccountingMeters
            { 500_00, AccountingMeters.BillCount500s },
            { 1_000_00, AccountingMeters.BillCount1_000s },
            { 2_000_00, AccountingMeters.BillCount2_000s },
            { 2_500_00, AccountingMeters.BillCount2_500s },
            { 5_000_00, AccountingMeters.BillCount5_000s },
            { 10_000_00, AccountingMeters.BillCount10_000s },
            { 20_000_00, AccountingMeters.BillCount20_000s },
            { 25_000_00, AccountingMeters.BillCount25_000s },
            { 50_000_00, AccountingMeters.BillCount50_000s },
            { 100_000_00, AccountingMeters.BillCount100_000s },
            { 200_000_00, "BillCount200000s" }, //not defined in AccountingMeters
            { 250_000_00, "BillCount250000s" }, //not defined in AccountingMeters
            { 500_000_00, "BillCount500000s" }, //not defined in AccountingMeters
            { 1_000_000_00, "BillCount1000000s" } //not defined in AccountingMeters
        };

        /// <summary>
        ///     Gets the meter name associated with the denomination of bill
        /// </summary>
        /// <param name="denomination">The denomination of bill</param>
        /// <returns>The name of the meter or null if there isn't a meter associated with the denomination</returns>
        public static string ToMeterName(long denomination)
        {
            return MeterNameMap.TryGetValue(denomination, out var meterName) ? meterName : string.Empty;
        }
    }
}