namespace Monaco.Application.Contracts.Metering
{
    /// <summary>
    ///     Defines the meter groups
    /// </summary>
    public enum MeterGroup
    {
        /// <summary>
        ///     This group includes the current value of the credit meters, counts for cabinet door openings, game play counts,
        ///     credit meter collection meters, bonus award meters, and total wagered meters for the EGM (broken down by funding
        ///     type).
        /// </summary>
        Cabinet,

        /// <summary>
        ///     This meter group includes meters related to actual game play on the EGM, such as wagered amount, won amount, and
        ///     games played. Meters in this group are available for each individual game on the EGM, gamePlay device meters, and
        ///     also as LTD meters for the EGM, gamePlay class meters.
        /// </summary>
        Performance,

        /// <summary>
        ///     This group includes a limited set of performance meters used to track the performance of different denominations at
        ///     the gamePlay device and class level.
        /// </summary>
        Denomination,

        /// <summary>
        ///     This group includes a limited set of performance meters used to track the performance of different wager categories
        ///     within a paytable at the gamePlay device level. These meters are not accumulated at the class level.
        /// </summary>
        WagerCategory,

        /// <summary>
        ///     This group includes meters related to the transfer of funds into the EGM, such as progressives, bonuses, and
        ///     wagering account transfers.
        /// </summary>
        TransferIn,

        /// <summary>
        ///     This group includes meters related to the transfer of funds out of the EGM, such as handpays, and wagering account
        ///     transfers.
        /// </summary>
        TransferOut,

        /// <summary>
        ///     This group includes meters related to the transfer of funds resulting from voucher transactions. For vouchers,
        ///     counts must be maintained by fund type, and the redemption of EGM-issued and system-issued vouchers at an EGM are
        ///     accounted for separately.
        /// </summary>
        Voucher,

        /// <summary>
        ///     This group contains the total amount of wager contributions to progressive devices (or class), as well as the count
        ///     of contributing games played.
        /// </summary>
        Progressive,

        /// <summary>
        ///     This group includes meters related to currency accepted by coin acceptors and note acceptors.
        /// </summary>
        CurrencyIn,

        /// <summary>
        ///     This group includes meters related to currency dispensed by coin hoppers and note dispensers.
        /// </summary>
        CurrencyOut
    }
}