namespace Aristocrat.Mgam.Client.Services.CreditServices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using Routing;

    /// <summary>
    ///     Implements <see cref="IVoucher"/> interface.
    /// </summary>
    internal class VoucherService : IVoucher, IDisposable
    {
        private readonly IRequestRouter _router;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherService"/> class.
        /// </summary>
        /// <param name="router"><see cref="IRequestRouter"/>.</param>
        /// <param name="services"><see cref="IHostServiceCollection"/>.</param>
        public VoucherService(
            IRequestRouter router,
            IHostServiceCollection services)
        {
            _router = router;

            services.Add(this);
        }

        /// <inheritdoc />
        ~VoucherService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public async Task<MessageResult<ValidateVoucherResponse>> ValidateVoucher(
            ValidateVoucher request,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<ValidateVoucher, ValidateVoucherResponse>(request, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<CreditResponse>> CreditVoucher(
            CreditVoucher request,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<CreditVoucher, CreditResponse>(request, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<VoucherPrintedResponse>> VoucherPrinted(
            VoucherPrinted request,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<VoucherPrinted, VoucherPrintedResponse>(request, cancellationToken);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _disposed = true;
        }
    }
}
