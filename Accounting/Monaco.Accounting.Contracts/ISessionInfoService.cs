namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;

    /// <summary>
    ///     Indicates the type of the session event
    /// </summary>
    public enum SessionEventType
    {
        /// <summary> None - Session default event</summary>
        None,

        /// <summary> The session started with a BillIn</summary>
        BillIn,

        /// <summary> The session started with a VoucherIn</summary>
        VoucherIn,

        /// <summary> The session started with a CurrencyIn</summary>
        CurrencyIn,

        /// <summary> The session ended by a VoucherOut</summary>
        VoucherOut,

        /// <summary> The session ended because of zero balance</summary>
        BalanceZero,

        /// <summary> The session ended because of handpay</summary>
        Handpay,

        /// <summary> The session ended because of wat</summary>
        Wat,

        /// <summary> The session started because of wat</summary>
        WatOn,

        /// <summary> The session ended by a HardMeterOut</summary>
        HardMeterOut,
    }

    /// <summary>
    ///     Provides information and services around session start and end
    /// </summary>
    public interface ISessionInfoService : IDisposable
    {
        /// <summary>
        ///     Get the session paid value.
        /// </summary>
        double GetSessionPaidValue();

        /// <summary>
        ///     Transaction to update the session.
        /// </summary>
        /// <param name="transaction"></param>
        void HandleTransaction(ITransaction transaction);
    }
}
