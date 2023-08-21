namespace Aristocrat.Mgam.Client.Services.Identification
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using Routing;

    /// <summary>
    ///     Allows the VLT to verify identification with the site controller.
    /// </summary>
    internal class IdentificationService : IIdentification
    {
        private readonly IRequestRouter _router;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdentificationService"/> class.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="router"></param>
        public IdentificationService(
            IHostServiceCollection services,
            IRequestRouter router)
        {
            _router = router;

            services.Add(this);
        }

        /// <inheritdoc />
        public async Task<MessageResult<EmployeeLoginResponse>> EmployeeLogin(
            string cardString,
            string pin,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<EmployeeLogin, EmployeeLoginResponse>(
                new EmployeeLogin
                {
                    CardString = cardString,
                    Pin = pin
                },
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<GetCardTypeResponse>> GetCardType(
            string cardStringTrack1,
            string cardStringTrack2,
            string cardStringTrack3,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<GetCardType, GetCardTypeResponse>(
                new GetCardType
                {
                    CardStringTrack1 = cardStringTrack1,
                    CardStringTrack2 = cardStringTrack2,
                    CardStringTrack3 = cardStringTrack3
                },
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<PlayerTrackingLoginResponse>> PlayerTrackingLogin(
            string cardString,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<PlayerTrackingLogin, PlayerTrackingLoginResponse>(
                new PlayerTrackingLogin
                {
                    PlayerTrackingString = cardString,
                },
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<PlayerTrackingLogoffResponse>> PlayerTrackingLogoff(
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<PlayerTrackingLogoff, PlayerTrackingLogoffResponse>(
                new PlayerTrackingLogoff(),
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<ChecksumResponse>> Checksum(
            Checksum request,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<Checksum, ChecksumResponse>(request, cancellationToken);
        }
    }
}
