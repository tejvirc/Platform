namespace Aristocrat.Mgam.Client.Services.Directory
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Logging;
    using Messaging;
    using Options;
    using Routing;

    /// <summary>
    ///     Manages communication with the Directory service on the host. 
    /// </summary>
    internal sealed class DirectoryService : IDirectory
    {
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<ProtocolOptions> _options;
        private readonly IBroadcastRouter _router;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryService"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/>.</param>
        /// <param name="router"><see cref="IBroadcastRouter"/>.</param>
        /// <param name="services"><see cref="IHostServiceCollection"/>.</param>
        public DirectoryService(
            ILogger<DirectoryService> logger,
            IOptionsMonitor<ProtocolOptions> options,
            IBroadcastRouter router,
            IHostServiceCollection services)
        {
            _logger = logger;
            _options = options;
            _router = router;

            services.Add(this);
        }

        /// <inheritdoc />
        public async Task<Guid> NewGuid(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Guid>();

            using(cancellationToken.Register(() => { tcs.TrySetCanceled(); }))
            {
                using (await _router.Broadcast(
                    new RequestGuid(_options.CurrentValue.DirectoryResponseAddress),
                    (RequestGuidResponse response) => tcs.TrySetResult(response.Guid),
                    cancellationToken))
                {
                    return tcs.Task.Result;
                }
            }
        }

        /// <inheritdoc />
        public async Task<IDisposable> LocateServices(string serviceName, Func<IPEndPoint, Task> listener)
        {
            _logger.LogDebug($"Requesting location for {serviceName}");

            return await _router.Broadcast(
                new RequestService(serviceName, _options.CurrentValue.DirectoryResponseAddress),
                (RequestServiceResponse response) =>
                {
                    var address = new Uri($"net.tcp://{response.ConnectionString}");
                    listener?.Invoke(new IPEndPoint(IPAddress.Parse(address.Host), address.Port));
                },
                CancellationToken.None);
        }

        /// <inheritdoc />
        public async Task<IDisposable> LocateXadf(string deviceName, string manufacturerName, Func<RequestXadfResponse, Task> listener)
        {
            _logger.LogDebug($"Requesting XADF for {deviceName} {manufacturerName}");

            return await _router.Broadcast(
                new RequestXadf(_options.CurrentValue.DirectoryResponseAddress, deviceName, manufacturerName), 
                (RequestXadfResponse response) =>
                {
                    listener?.Invoke(response);
                },
                CancellationToken.None);
        }
    }
}
