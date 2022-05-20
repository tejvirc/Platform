namespace Aristocrat.Monaco.Mgam.Middleware
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Routing;
    using Common.Data.Models;
    using Common.Data.Repositories;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Adds the current session ID to the message.
    /// </summary>

    public class SessionMiddleware : IRequestRouter
    {
        private readonly IRequestRouter _next;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionMiddleware"/> class.
        /// </summary>
        /// <param name="next"><see cref="IRequestRouter"/>.</param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory"/>.</param>
        public SessionMiddleware(
            IRequestRouter next,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _next = next;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task<MessageResult<TResponse>> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken)
            where TRequest : class, IRequest
            where TResponse : class, IResponse
        {
            if (request is ISessionId sessionMessage)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var session = unitOfWork.Repository<Session>().GetSessionId();

                    if (session == null)
                    {
                        throw new ServerResponseException(ServerResponseCode.InvalidSessionId);
                    }

                    sessionMessage.SessionId = session.Value;
                }
            }

            return await _next.Send<TRequest, TResponse>(request, cancellationToken);
        }
    }
}
