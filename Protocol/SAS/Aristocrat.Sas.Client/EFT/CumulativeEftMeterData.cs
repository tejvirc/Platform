namespace Aristocrat.Sas.Client.EFT
{
    /// <summary>
    ///     Gaming machines that support EFT must track all cashable, non-cashable, and promotional credits
    ///     transferred to the gaming machine from the host and all credits transferred to the host
    ///     from the gaming machine in cumulative meters. The host can obtain these meters by issuing a
    ///     type R long poll with command code 1D
    /// </summary>
    public class CumulativeEftMeterData : LongPollResponse
    {
        /// <summary>
        ///     Cumulative promotional credits transferred to the gaming machine.
        /// </summary>
        public ulong PromotionalCredits { get; set; }

        /// <summary>
        ///     Cumulative non-cashable credits transferred to the gaming machine.
        /// </summary>
        public ulong NonCashableCredits { get; set; }

        /// <summary>
        ///     Cumulative cashable credits transferred to the gaming machine.
        /// </summary>
        public ulong CashableCredits { get; set; }

        /// <summary>
        ///     Cumulative credits transferred to the SAS HOST, includes cashable, non-cashable and promotional.
        /// </summary>
        public ulong TransferredCredits { get; set; }
    }
}