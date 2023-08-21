namespace Aristocrat.Mgam.Client.Middleware
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using Routing;

    /// <summary>
    ///     Adds the current instance ID to the message.
    /// </summary>
    public class InstanceMiddleware : IRequestRouter
    {
        private readonly IRequestRouter _next;
        private readonly IEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InstanceMiddleware"/> class.
        /// </summary>
        /// <param name="next"><see cref="IRequestRouter"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        public InstanceMiddleware(
            IRequestRouter next,
            IEgm egm)
        {
            _next = next;
            _egm = egm;
        }

        /// <inheritdoc />
        public async Task<MessageResult<TResponse>> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken) where TRequest : class, IRequest where TResponse : class, IResponse
        {
            if (request is IInstanceId instanceMessage)
            {
                var instanceId = _egm.ActiveInstance?.InstanceId;
                if (instanceId == null)
                {
                    throw new ServerResponseException(ServerResponseCode.InvalidInstanceId);
                }

                instanceMessage.InstanceId = instanceId.Value;
            }

            return await _next.Send<TRequest, TResponse>(request, cancellationToken);
        }
    }
}
