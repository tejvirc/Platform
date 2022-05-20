namespace Aristocrat.Monaco.Gaming.Contracts.Barkeeper
{
    /// <summary>
    ///     The handler for the barkeeper functionality
    /// </summary>
    public interface IBarkeeperHandler
    {
        /// <summary>
        /// </summary>
        BarkeeperRewardLevels RewardLevels { get; set; }

        /// <summary>
        ///     Gets the current credits in amount for the barkeeper session
        /// </summary>
        long CreditsInDuringSession { get; }

        /// <summary>
        ///     Gets the current coin in amount for the barkeeper session
        /// </summary>
        long CoinInDuringSession { get; }

        /// <summary>
        ///     Updates the barkeeper handler for audit mode entry
        /// </summary>
        void OnAuditEntered();

        /// <summary>
        ///     Updates the barkeeper handler for audit mode exit
        /// </summary>
        void OnAuditExited();

        /// <summary>
        ///     Updates the barkeeper handler for bank balance changes
        /// </summary>
        /// <param name="newBalance">The new bank balance</param>
        void OnBalanceUpdate(long newBalance);

        /// <summary>
        ///     Used to tell the barkeeper handler that a cashout operator has completed
        /// </summary>
        void OnCashOutCompleted();

        /// <summary>
        ///     Used to tell the barkeeper handler of the total being added to the credit meter as a result of credits being
        ///     inserted
        /// </summary>
        /// <param name="total">The total being added to the credit meter</param>
        void OnCreditsInserted(long total);

        /// <summary>
        ///     Used to tell the barkeeper handler the amount that is being wagered
        /// </summary>
        /// <param name="wageredAmount">The total amount wagered</param>
        void CreditsWagered(long wageredAmount);

        /// <summary>
        ///     Used to tell the barkeeper handler that a game round has ended
        /// </summary>
        /// <param name="gameHistory"></param>
        void GameEnded(IGameHistoryLog gameHistory);

        /// <summary>
        ///     Called when the barkeeper button has been pressed
        /// </summary>
        void BarkeeperButtonPressed();
    }
}