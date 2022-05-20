namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Defines the cancellation reasons
    /// </summary>
    public enum CancellationReason
    {
        /// <summary>
        ///     Cancel for any reason
        /// </summary>
        Any,

        /// <summary>
        ///     The player is no longer valid
        /// </summary>
        IdInvalidated,

        /// <summary>
        ///     The credit meter is zero
        /// </summary>
        ZeroCredits
    }

    /// <summary>
    ///     Defines a bonus strategy
    /// </summary>
    public interface IBonusStrategy
    {
        /// <summary>
        ///     Creates a bonus transaction
        /// </summary>
        /// <typeparam name="T">The request type</typeparam>
        /// <param name="deviceId">The associated device Id</param>
        /// <param name="request">The bonus award request</param>
        /// <returns></returns>
        BonusTransaction CreateTransaction<T>(int deviceId, T request) where T : IBonusRequest;

        /// <summary>
        ///     Attempts to pay the provided bonus transaction
        /// </summary>
        /// <param name="transaction">The bonus transaction to pay</param>
        /// <returns>true if the bonus is eligible to be paid</returns>
        bool CanPay(BonusTransaction transaction);

        /// <summary>
        ///     Attempts to pay the provided bonus transaction
        /// </summary>
        /// <param name="transaction">The bonus transaction to pay</param>
        /// <param name="transactionId">The bank transaction used to pay the bonus</param>
        /// <param name="context">The continuation context</param>
        /// <returns>A type-specific continuation context</returns>
        Task<IContinuationContext> Pay(BonusTransaction transaction, Guid transactionId, IContinuationContext context);

        /// <summary>
        ///     Attempts to cancel a transaction
        /// </summary>
        /// <param name="transaction">The bonus transaction to cancel</param>
        /// <returns>return true, if the bonus was cancelled</returns>
        bool Cancel(BonusTransaction transaction);

        /// <summary>
        ///     Attempts to cancel a transaction
        /// </summary>
        /// <param name="transaction">The bonus transaction to cancel</param>
        /// <param name="reason">The cancellation reason</param>
        /// <returns>return true, if the bonus was cancelled</returns>
        bool Cancel(BonusTransaction transaction, CancellationReason reason);

        /// <summary>
        ///     Attempts to recover the specified transaction, if needed
        /// </summary>
        /// <param name="transaction">The bonus transaction to recover</param>
        /// <param name="transactionId">The bank transaction used to pay the bonus</param>
        Task Recover(BonusTransaction transaction, Guid transactionId);
    }
}