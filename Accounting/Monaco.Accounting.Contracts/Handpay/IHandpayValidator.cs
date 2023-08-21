namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     An interface for validating a handpay.
    /// </summary>
    public interface IHandpayValidator: IService
    {
        /// <summary>
        ///     Gets a value indicating whether local handpay is allowed
        /// </summary>
        bool AllowLocalHandpay { get; }

        /// <summary>
        ///     Gets a value indicating whether a validating host is online or not
        /// </summary>
        bool HostOnline { get; }

        /// <summary>
        ///     Determines whether the transaction should be logged
        /// </summary>
        /// <param name="transaction">The transaction.  Optional.</param>
        /// <returns>True if the transaction should be logged</returns>
        bool LogTransactionRequired(ITransaction transaction = null);

        /// <summary>
        ///     Provides a mechanism to validate a handpay
        /// </summary>
        /// <param name="cashableAmount">The cashable amount requiring a handpay</param>
        /// <param name="promoAmount">The promotional amount requiring a handpay</param>
        /// <param name="nonCashAmount">The non-cashable amount requiring a handpay</param>
        /// <param name="handpayType">The handpay type</param>
        /// <returns>Whether or not handpays can be validated</returns>
        bool ValidateHandpay(
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            HandpayType handpayType);

        /// <summary>
        ///     Method to start a handpay request.
        /// </summary>
        /// <param name="transaction">The handpay transaction</param>
        Task RequestHandpay(HandpayTransaction transaction);

        /// <summary>
        ///     Method to execute after a handpay is keyed-off.  Used by protocols to perform additional updates to
        ///     the given handpay transaction.
        /// </summary>
        /// <param name="transaction">The handpay transaction that has been keyed-off</param>
        Task HandpayKeyedOff(HandpayTransaction transaction);
    }
}