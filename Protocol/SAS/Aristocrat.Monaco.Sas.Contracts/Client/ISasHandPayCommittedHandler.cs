namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System.Threading.Tasks;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     The handpay committed handler for SAS
    /// </summary>
    public interface ISasHandPayCommittedHandler
    {
        /// <summary>
        ///     Process any pending handpay notifications that need to be posted
        /// </summary>
        /// <param name="transaction"></param>
        Task HandpayPending(HandpayTransaction transaction);

        /// <summary>
        ///     Process the provided handpay reset
        /// </summary>
        /// <param name="transaction">The handpay that was reset</param>
        void HandPayReset(HandpayTransaction transaction);

        /// <summary>
        ///     Gets the next unread handpay transaction or null if there are no unread transactions
        /// </summary>
        /// <param name="clientNumber"></param>
        /// <returns>the next unread handpay transaction or null if there are no unread transactions</returns>
        LongPollHandpayDataResponse GetNextUnreadHandpayTransaction(byte clientNumber);

        /// <summary>
        ///     Registers the handpay queue for handling any handpay events from the platform
        /// </summary>
        /// <param name="handpayQueue">The queue to register</param>
        /// <param name="clientNumber">The client number for this handpay queue</param>
        void RegisterHandpayQueue(IHandpayQueue handpayQueue, byte clientNumber);

        /// <summary>
        /// Un-Registers the handpay queue from listing to any handpay events
        /// </summary>
        /// <param name="handpayQueue">The queue to un-register</param>
        /// <param name="clientNumber">The client number for this handpay queue</param>
        void UnRegisterHandpayQueue(IHandpayQueue handpayQueue, byte clientNumber);

        /// <summary>
        ///     Recovers any pending handpay exception handling
        /// </summary>
        void Recover();
    }
}