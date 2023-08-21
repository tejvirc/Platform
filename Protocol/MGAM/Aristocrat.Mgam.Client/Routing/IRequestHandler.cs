namespace Aristocrat.Mgam.Client.Routing
{
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    ///     Dispatches message requests and commands send from the host.
    /// </summary>
    public interface IRequestHandler
    {
        /// <summary>
        ///     Dispatches a request from the host to a message handler.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IResponse> Receive<TRequest>(TRequest request)
            where TRequest : IRequest;
    }
}
