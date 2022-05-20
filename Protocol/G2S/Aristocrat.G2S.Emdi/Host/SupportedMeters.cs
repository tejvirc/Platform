namespace Aristocrat.G2S.Emdi.Host
{
    using Meters;
    using Protocol.v21ext1b1;
    using System.Collections.Generic;

    /// <summary>
    /// Returns a list of supported meters
    /// </summary>
    internal static class SupportedMeters
    {
        /// <summary>
        /// Gets a ist of supported events
        /// </summary>
        /// <returns>List of supported events</returns>
        public static IList<(string Name, t_meterTypes Type)> Get()
        {
            return new List<(string Name, t_meterTypes Type)>
            {
                (MeterNames.PlayerPointBalance, t_meterTypes.IGT_count),
                (MeterNames.PlayerPointCountdown, t_meterTypes.IGT_count),
                (MeterNames.PlayerSessionPoints, t_meterTypes.IGT_count),
                (MeterNames.WagerMatchBalance, t_meterTypes.IGT_amount),
                (MeterNames.G2SPlayerCashableAmt, t_meterTypes.IGT_amount),
                (MeterNames.G2SPlayerPromoAmt, t_meterTypes.IGT_amount),
                (MeterNames.G2SPlayerNonCashAmt, t_meterTypes.IGT_amount),
            };
        }
    }
}
