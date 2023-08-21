namespace Aristocrat.Mgam.Client.Services.Session
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using Routing;

    /// <summary>
    ///     Implements <see cref="ISession"/> interface.
    /// </summary>
    internal class SessionService : ISession
    {
        private readonly IRequestRouter _router;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionService"/> class.
        /// </summary>
        /// <param name="router"><see cref="IRequestRouter"/>.</param>
        /// <param name="services"><see cref="IHostServiceCollection"/>.</param>
        public SessionService(
            IRequestRouter router,
            IHostServiceCollection services)
        {
            _router = router;

            services.Add(this);
        }

        /// <inheritdoc />
        public async Task<MessageResult<BeginSessionResponse>> BeginSession(
            BeginSession request,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<BeginSession, BeginSessionResponse>(request, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MessageResult<BeginSessionResponse>> BeginSessionWithSessionId(
            BeginSessionWithSessionId request,
            CancellationToken cancellationToken)
        {
            return await _router.Send<BeginSessionWithSessionId, BeginSessionResponse>(request, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<MessageResult<EndSessionResponse>> EndSession(
            EndSession request,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<EndSession, EndSessionResponse>(request, cancellationToken);
        }

        public async Task<MessageResult<RequestPlayResponse>> RequestPlay(
            RequestPlay request,
            CancellationToken cancellationToken = default)
        {
            return await _router.Send<RequestPlay, RequestPlayResponse>(request, cancellationToken);
        }
    }
}
