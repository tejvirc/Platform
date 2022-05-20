namespace Aristocrat.Mgam.Client.Services.DropMode
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Logging;
    using Messaging;
    using Routing;

    /// <summary>
    ///     Implements <see cref="IBillAcceptorMeter"/> interface.
    /// </summary>
    internal class BillAcceptorMeterService : IBillAcceptorMeter, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IRequestRouter _router;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BillAcceptorMeterService"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="router"><see cref="IRequestRouter"/>.</param>
        /// <param name="services"><see cref="IHostServiceCollection"/>.</param>
        public BillAcceptorMeterService(
            ILogger<BillAcceptorMeterService> logger,
            IRequestRouter router,
            IHostServiceCollection services)
        {
            _logger = logger;
            _router = router;

            services.Add(this);
        }

        /// <inheritdoc />
        ~BillAcceptorMeterService()
        {
            Dispose(false);
        }

        public async Task<MessageResult<BillAcceptorMeterReportResponse>> ReportMeters(
            BillAcceptorMeterReport request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Sending BillAcceptorMeterReport to server.");

            var response = await _router.Send<BillAcceptorMeterReport, BillAcceptorMeterReportResponse>(request, cancellationToken);

            _logger.LogDebug("Received BillAcceptorMeterReportResponse from server.");

            return response;
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
