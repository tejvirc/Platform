namespace Aristocrat.Mgam.Client.Services.Session
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Provides interface for session related interactions with the host.
    /// </summary>
    public interface ISession : IHostService
    {
        /// <summary>
        ///     Begin session.
        /// </summary>
        /// <param name="request">Begin session.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="BeginSessionResponse"/>.</returns>
        Task<MessageResult<BeginSessionResponse>> BeginSession(
            BeginSession request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Begin session with session id.
        /// </summary>
        /// <param name="request">Begin session with session id.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="BeginSessionResponse"/>.</returns>
        Task<MessageResult<BeginSessionResponse>> BeginSessionWithSessionId(
            BeginSessionWithSessionId request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     End session.
        /// </summary>
        /// <param name="request">End session.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="EndSessionResponse"/>.</returns>
        Task<MessageResult<EndSessionResponse>> EndSession(
            EndSession request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Request play.
        /// </summary>
        /// <param name="request">Request play.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task with <see cref="IResponse"/>.  The response will either be <see cref="RequestPlayResponse"/> or <see cref="RequestPlayVoucherResponse"/>.</returns>
        Task<MessageResult<RequestPlayResponse>> RequestPlay(
            RequestPlay request,
            CancellationToken cancellationToken = default);
    }
}
