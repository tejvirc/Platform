namespace Aristocrat.Mgam.Client
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Options;
    using Services;

    /// <summary>
    ///     Gateway interface for interacting with client services.
    /// </summary>
    public interface IEgm
    {
        /// <summary>
        ///     Gets the the state of the protocol client.
        /// </summary>
        EgmState State { get; }

        /// <summary>
        ///     Gets the configured protocol options.
        /// </summary>
        IOptionsMonitor<ProtocolOptions> Options { get; }

        /// <summary>
        ///     Gets the active instance returned in the RegisterInstanceResponse message.
        /// </summary>
        InstanceInfo ActiveInstance { get; }

        /// <summary>
        ///     Sets the active instance after a successfully registering with the VLT service.
        /// </summary>
        /// <param name="instance">Instance information.</param>
        void SetActiveInstance(InstanceInfo instance);

        /// <summary>
        ///     Clears the active instance.
        /// </summary>
        void ClearActiveInstance();

        /// <summary>
        ///     Starts client services.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Start(CancellationToken cancellationToken);

        /// <summary>
        ///     Stops client services.
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task Stop(CancellationToken cancellationToken);

        /// <summary>
        ///     Gets a reference to a <see cref="IHostService"/> instance.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetService<TService>()
            where TService : IHostService;

        /// <summary>
        ///     Sends a message the VLT service to keep the connection active.
        /// </summary>
        Task KeepAlive(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Connect to VLT Service end point.
        /// </summary>
        /// <param name="endPoint">VLT Service endpoint location.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="Task"/>. True if connection was opened successfully.</returns>
        Task Connect(IPEndPoint endPoint, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Disconnect from VLT Service end point.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task Disconnect(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Unregister the active instance.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="Task"/>. True if unregistered.</returns>
        Task<bool> Unregister(CancellationToken cancellationToken = default);
    }
}
