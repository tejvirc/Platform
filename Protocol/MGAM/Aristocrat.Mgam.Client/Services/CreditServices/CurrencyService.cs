namespace Aristocrat.Mgam.Client.Services.CreditServices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using Routing;

    /// <summary>
    ///     Implements <see cref="ICurrency"/> interface.
    /// </summary>
    internal class CurrencyService : ICurrency, IDisposable
    {
        private readonly IRequestRouter _router;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyService"/> class.
        /// </summary>
        /// <param name="router"><see cref="IRequestRouter"/>.</param>
        /// <param name="services"><see cref="IHostServiceCollection"/>.</param>
        public CurrencyService(
            IRequestRouter router,
            IHostServiceCollection services)
        {
            _router = router;

            services.Add(this);
        }

        /// <inheritdoc />
        ~CurrencyService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public async Task<MessageResult<EscrowCashResponse>> EscrowCash(
            EscrowCash request,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<EscrowCash, EscrowCashResponse>(request, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<CreditResponse>> CreditCash(
            CreditCash request,
            CancellationToken cancellationToken)
        {
            return await _router.Send<CreditCash, CreditResponse>(request, cancellationToken);
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
