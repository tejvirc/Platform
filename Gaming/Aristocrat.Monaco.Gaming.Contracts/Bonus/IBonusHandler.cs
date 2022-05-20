namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;
    using System.Collections.Generic;

    /// <summary>The IBonusHandler provides functions for awarding bonuses.</summary>
    public interface IBonusHandler : IGameDelay
    {
        /// <summary>
        ///     Gets the current transaction list
        /// </summary>
        IReadOnlyCollection<BonusTransaction> Transactions { get; }

        /// <summary>
        ///     Gets of sets the maximum number of pending devices
        /// </summary>
        int MaxPending { get; set; }

        /// <summary>
        ///     Creates a standard bonus award
        /// </summary>
        /// <param name="request">A bonus request</param>
        /// <returns>a <see cref="BonusTransaction" /> if the request was successful</returns>
        BonusTransaction Award<T>(T request) where T : IBonusRequest;

        /// <summary>
        ///     Marks the specified transaction as acknowledged
        /// </summary>
        /// <param name="transactionId">The unique transaction identifier</param>
        void Acknowledge(long transactionId);

        /// <summary>
        ///     Marks the specified transaction as acknowledged
        /// </summary>
        /// <param name="bonusId">The host assigned bonus identifier</param>
        void Acknowledge(string bonusId);

        /// <summary>
        ///     Commits pending bonuses based on the current game play state
        /// </summary>
        /// <returns>returns true if any bonuses are eligible to be committed</returns>
        bool Commit();

        /// <summary>
        ///     Commits pending bonuses based on the current game play state
        /// </summary>
        /// <param name="transactionId">The bank transaction associated with this commit</param>
        /// <returns>returns true if any bonuses are eligible to be committed</returns>
        bool Commit(Guid transactionId);

        /// <summary>
        ///     Cancels the specified transaction
        /// </summary>
        /// <param name="transactionId">The unique transaction identifier</param>
        /// <returns>true if successful, otherwise false</returns>
        bool Cancel(long transactionId);

        /// <summary>
        ///     Cancels the specified transaction
        /// </summary>
        /// <param name="bonusId">The host assigned bonus identifier</param>
        /// <returns>true if successful, otherwise false</returns>
        bool Cancel(string bonusId);

        /// <summary>
        ///     Checks to see if the specified transaction is in the current log
        /// </summary>
        /// <param name="transactionId">The unique transaction identifier</param>
        /// <returns>true if exists, otherwise false</returns>
        bool Exists(long transactionId);

        /// <summary>
        ///     Checks to see if the specified transaction is in the current log
        /// </summary>
        /// <param name="bonusId">The host assigned bonus identifier</param>
        /// <returns>true if exists, otherwise false</returns>
        bool Exists(string bonusId);
    }
}