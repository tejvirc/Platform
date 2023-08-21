namespace Aristocrat.Mgam.Client.Services.Identification
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     
    /// </summary>
    public interface IIdentification : IHostService
    {
        /// <summary>
        ///     Login the employee.
        /// </summary>
        /// <param name="cardString">The card returned from a GetCardTypeResponse.</param>
        /// <param name="pin">The employee's pin.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The GetCardTypeResponse</returns>
        Task<MessageResult<EmployeeLoginResponse>> EmployeeLogin(
            string cardString,
            string pin,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets the card type.
        /// </summary>
        /// <param name="cardStringTrack1">The card track 1 string.</param>
        /// <param name="cardStringTrack2">The card track 2 string.</param>
        /// <param name="cardStringTrack3">The card track 3 string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The GetCardTypeResponse</returns>
        Task<MessageResult<GetCardTypeResponse>> GetCardType(
            string cardStringTrack1,
            string cardStringTrack2,
            string cardStringTrack3,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs the player in to a tracking session.
        /// </summary>
        /// <param name="cardStringTrack1">The card track 1 string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The GetCardTypeResponse</returns>
        Task<MessageResult<PlayerTrackingLoginResponse>> PlayerTrackingLogin(
            string cardStringTrack1,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs the player out of a tracking session
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The GetCardTypeResponse</returns>
        Task<MessageResult<PlayerTrackingLogoffResponse>> PlayerTrackingLogoff(
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Software checksum.
        /// </summary>
        /// <param name="request">The software checksum request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The ChecksumResponse.</returns>
        Task<MessageResult<ChecksumResponse>> Checksum(
            Checksum request,
            CancellationToken cancellationToken = default);
    }
}
