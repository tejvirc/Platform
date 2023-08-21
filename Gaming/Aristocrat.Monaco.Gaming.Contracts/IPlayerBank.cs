namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Accounting.Contracts.TransferOut;

    /// <summary>
    ///     Provides a mechanism for interacting with the player funds
    /// </summary>
    public interface IPlayerBank
    {
        /// <summary>
        ///     Gets the players current balance in millicents
        /// </summary>
        long Balance { get; }

        /// <summary>
        ///     Gets the players current balance in credits
        /// </summary>
        long Credits { get; }

        /// <summary>
        ///     Gets the current transaction identifier
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        ///     Locks the player bank. Will wait until a transaction can be acquired
        /// </summary>
        void WaitForLock();

        /// <summary>
        ///     Locks the player bank
        /// </summary>
        /// <returns>true if the lock was obtained.</returns>
        bool Lock();

        /// <summary>
        ///     Locks the player bank
        /// </summary>
        /// <param name="timeout">The timeout for the lock request</param>
        /// <returns>true if the lock was obtained.</returns>
        bool Lock(TimeSpan timeout);

        /// <summary>
        ///     Unlocks the player bank
        /// </summary>
        void Unlock();

        /// <summary>
        ///     Subtracts the specified amount from the players balance as a game wager.
        /// </summary>
        /// <param name="amount">The bet amount in credits.</param>
        void Wager(long amount);

        /// <summary>
        ///     Adds the specified win amount to the players balances.
        /// </summary>
        /// <param name="amount">The win amount in credits.</param>
        void AddWin(long amount);

        /// <summary>
        ///     Cashes out the player bank, which includes all remaining credits in the bank.
        /// </summary>
        /// <returns>true if the cashout completed successfully, else false.</returns>
        bool CashOut();

        /// <summary>
        ///     Cashes out the player bank, which includes all remaining credits in the bank.
        /// </summary>
        /// <param name="forcedCashout">True if this is a forced cash out. Typically due to exceeding a jurisdictional limit.</param>
        /// <returns>true if the cashout completed successfully, else false.</returns>
        bool CashOut(bool forcedCashout);

        /// <summary>
        ///     Cashes out the player bank, which includes all remaining credits in the bank.
        /// </summary>
        /// <param name="amount">The amount to cash out. It's a partial cash out.</param>
        /// <param name="forcedCashout">true if this is a forced cash out.  Typically due to exceeding a jurisdictional limit</param>
        /// <returns>true if the cashout completed successfully, else false.</returns>
        bool CashOut(long amount, bool forcedCashout = false);

        /// <summary>
        ///     Cashes out the player bank, which includes all remaining credits in the bank.
        /// </summary>
        /// <param name="traceId">A reference Id that should be associated with the cash out</param>
        /// <param name="forcedCashout">true if this is a forced cash out.  Typically due to exceeding a jurisdictional limit</param>
        /// <param name="associatedTransaction">The associated transaction Id.  This will typically be something like the gamePlay transaction</param>
        /// <returns>true if the cashout completed successfully, else false.</returns>
        bool CashOut(Guid traceId, bool forcedCashout, long associatedTransaction);

        /// <summary>
        ///     Cashes out the player bank for the amount specified
        /// </summary>
        /// <returns>true if the cashout completed successfully, else false.</returns>
        /// <param name="traceId">A reference Id that should be associated with the cash out</param>
        /// <param name="amount">The amount to cash out. It's a partial cash out.</param>
        /// <param name="forcedCashout">true if this is a forced cash out.  Typically due to exceeding a jurisdictional limit</param>
        /// <param name="reason">The transfer out reason</param>
        /// <param name="associatedTransaction">The associated transaction Id.  This will typically be something like the gamePlay transaction</param>
        /// <returns>true if the partial cashout completed successfully, else false.</returns>
        bool CashOut(Guid traceId, long amount, TransferOutReason reason, bool forcedCashout, long associatedTransaction);

        /// <summary>
        ///     Forces a handpay for the amount specified
        /// </summary>
        /// <param name="traceId">A reference Id that should be associated with the cash out</param>
        /// <param name="amount">The amount to handpay</param>
        /// <param name="reason">The transfer out reason</param>
        /// <param name="associatedTransaction">The associated transaction Id.  This will typically be something like the gamePlay transaction</param>
        /// <returns>true if the handpay completed successfully, else false.</returns>
        bool ForceHandpay(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction);

        /// <summary>
        ///     Forces a voucher out for the amount specified.
        /// </summary>
        /// <param name="traceId">A reference Id that should be associated with the cash out.</param>
        /// <param name="amount">The amount to cash out. It's a partial cash out.</param>
        /// <param name="reason">The transfer out reason.</param>
        /// <param name="associatedTransaction">The associated transaction Id.  This will typically be something like the gamePlay transaction.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        bool ForceVoucherOut(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction);
    }
}
