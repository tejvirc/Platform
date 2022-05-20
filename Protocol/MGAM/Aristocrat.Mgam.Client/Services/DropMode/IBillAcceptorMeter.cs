namespace Aristocrat.Mgam.Client.Services.DropMode
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Provides interface for bill-acceptor meters related interactions with the host.
    /// </summary>
    public interface IBillAcceptorMeter : IHostService
    {
        /// <summary>
        ///     Send BillAcceptorMeter report.
        /// </summary>
        /// <param name="request">Report.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="IResponse"/>.  The response will be <see cref="BillAcceptorMeterReportResponse"/>.</returns>
        Task<MessageResult<BillAcceptorMeterReportResponse>> ReportMeters(
            BillAcceptorMeterReport request,
            CancellationToken cancellationToken = default);
    }
}
