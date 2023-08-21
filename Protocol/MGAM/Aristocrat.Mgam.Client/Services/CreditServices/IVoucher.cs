namespace Aristocrat.Mgam.Client.Services.CreditServices
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Provides interface for voucher related interactions with the host.
    /// </summary>
    public interface IVoucher : IHostService
    {
        /// <summary>
        ///     Validate Voucher.
        /// </summary>
        /// <param name="request">Validate voucher.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="ValidateVoucherResponse"/>.</returns>
        Task<MessageResult<ValidateVoucherResponse>> ValidateVoucher(
            ValidateVoucher request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Credit Voucher.
        /// </summary>
        /// <param name="request">Credit voucher.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="CreditResponse"/>.</returns>
        Task<MessageResult<CreditResponse>> CreditVoucher(
            CreditVoucher request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Voucher Printed.
        /// </summary>
        /// <param name="request">Voucher printed.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="VoucherPrintedResponse"/>.</returns>
        Task<MessageResult<VoucherPrintedResponse>> VoucherPrinted(
            VoucherPrinted request,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
