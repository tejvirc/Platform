namespace Aristocrat.Mgam.Client.Services.CreditServices
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Provides interface for currency related interactions with the host.
    /// </summary>
    public interface ICurrency : IHostService
    {
        /// <summary>
        ///     Escrow Cash.
        /// </summary>
        /// <param name="request">Escrow Cash.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="EscrowCashResponse"/>.</returns>
        Task<MessageResult<EscrowCashResponse>> EscrowCash(
            EscrowCash request,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Credit Cash.
        /// </summary>
        /// <param name="request">Credit Cash.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="CreditResponse"/>.</returns>
        Task<MessageResult<CreditResponse>> CreditCash(
            CreditCash request,
            CancellationToken cancellationToken = default(CancellationToken));

    }
}
