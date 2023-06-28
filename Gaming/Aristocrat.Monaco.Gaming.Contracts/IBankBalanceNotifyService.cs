namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Notifies runtime the balance updates
    /// </summary>
    public interface IBalanceUpdateService
    {
        /// <summary>
        ///     Sends runtime the updated balance
        /// </summary>
        void UpdateBalance();
    }
}