namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    /// <summary>
    ///     The different types of host cashout modes
    /// </summary>
    public enum HostCashOutMode
    {
        /// <summary>
        ///     There is no current set host cashout mode
        /// </summary>
        None,
        /// <summary>
        ///     Soft cashout will proceed to the next available cashout device upon host cashout failure
        /// </summary>
        Soft,
        /// <summary>
        ///     Hard cashout will proceed to lock up the EGM awaiting a key off upon host cashout failure
        /// </summary>
        Hard
    }
}