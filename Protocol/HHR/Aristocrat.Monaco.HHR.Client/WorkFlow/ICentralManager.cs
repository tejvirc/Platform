namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Messages;

    /// <summary>
    /// </summary>
    public interface ICentralManager
    {
        /// <summary>
        ///     Sends a request to Central Server and Expects a response in return
        /// </summary>
        /// <typeparam name="TRequest">Request type to send</typeparam>
        /// <typeparam name="TResponse">Response type to expect</typeparam>
        /// <param name="request">Request to server</param>
        /// <param name="token">Cancellation token to make it cancellable.</param>
        /// <returns>Expected Response returned from server</returns>
        /// <exception cref="UnexpectedResponseException">If server doesn't return expected response.</exception>
        Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken token = default)
            where TRequest : Request where TResponse : Response;

        /// <summary>
        ///     For requests where no response is expected.
        /// </summary>
        /// <param name="request">Request to send to server.</param>
        /// <param name="token">Cancellation token to make this operation cancellable.</param>
        /// <returns></returns>
        Task Send<TRequest>(TRequest request, CancellationToken token = default)
            where TRequest : Request;

        /// <summary>
        ///     CentralServer can send a Responses without EGM sending any request.
        ///     Anyone interested in CentralServer unsolicited responses can subscribe for this and it will be notified as CentralServer sends a response.
        /// </summary>
        IObservable<Response> UnsolicitedResponses { get; }

        /// <summary>
        ///     Request observable, which will notify when a request is sent to central server.
        /// </summary>
        IObservable<(Request request, Type responseType)> RequestObservable { get; }

        /// <summary>
        ///     Response observable, which will notify response received for a request.
        /// </summary>
        IObservable<(Request, Response)> RequestResponseObservable { get; }

        /// <summary>
        ///     Event when the request is modified to contain correct sequence ID
        /// </summary>
        event EventHandler<Request> RequestModifiedHandler;
    }
}