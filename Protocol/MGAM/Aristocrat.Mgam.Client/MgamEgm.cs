namespace Aristocrat.Mgam.Client
{
    using Logging;
    using Messaging;
    using Options;
    using Polly;
    using Routing;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Manages interaction with consumers of the client library components.
    /// </summary>
    internal sealed class MgamEgm : IEgm
    {
        private const int AllowCleanupDelay = 1000;
        private const int ShutdownTimeout = 6;
        private const int ConnectionTimeout = 15;

        private readonly ILogger _logger;
        private readonly IEnumerable<IStartable> _startables;
        private readonly IHostServiceCollection _services;
        private readonly ISecureTransporter _transporter;
        private readonly IHostQueue _queue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MgamEgm"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/>.</param>
        /// <param name="startables"><see cref="IStartable"/>.</param>
        /// <param name="services"><see cref="IHostServiceCollection"/>.</param>
        /// <param name="transporter"><see cref="ISecureTransporter"/>.</param>
        /// <param name="queue"><see cref="IHostQueue"/>.</param>
        public MgamEgm(
            ILogger<MgamEgm> logger,
            IOptionsMonitor<ProtocolOptions> options,
            IEnumerable<IStartable> startables,
            IHostServiceCollection services,
            ISecureTransporter transporter,
            IHostQueue queue)
        {
            _logger = logger;
            Options = options;
            _startables = startables;
            _services = services;
            _transporter = transporter;
            _queue = queue;
        }

        /// <inheritdoc />
        public EgmState State { get; private set; } = EgmState.NotStarted;

        /// <inheritdoc />
        public IOptionsMonitor<ProtocolOptions> Options { get; }

        /// <inheritdoc />
        public InstanceInfo ActiveInstance { get; private set; }

        /// <inheritdoc />
        public void SetActiveInstance(InstanceInfo instance)
        {
            ActiveInstance = instance;
        }

        public void ClearActiveInstance()
        {
            ActiveInstance = null;
        }

        /// <inheritdoc />
        public async Task Start(CancellationToken cancellationToken)
        {
            _logger.LogInfo($"Starting {GetType().Name}...");

            State = EgmState.Starting;

            try
            {
                foreach (var startable in _startables)
                {
                    await Start(startable);
                }
            }
            catch (Exception ex)
            {
                State = EgmState.Failure;
                _logger.LogError(ex, $"Starting {nameof(MgamEgm)}.");
                return;
            }

            State = EgmState.Started;

            _logger.LogInfo($"Started {GetType().Name}...");
        }

        /// <inheritdoc />
        public async Task Stop(CancellationToken cancellationToken)
        {
            _logger.LogInfo($"Stopping {GetType().Name}...");

            State = EgmState.Stopping;

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(ShutdownTimeout)))
            {
                using (var lts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                {
                    try
                    {
                        await Disconnect(lts.Token);
                    }
                    catch (Exception ex)
                    {
                        // Unconditionally catch all and continue
                        _logger.LogError(ex, "Disconnect failure.");
                    }
                }

                foreach (var startable in _startables)
                {
                    await Stop(startable);
                }
            }

            await Task.Delay(AllowCleanupDelay, cancellationToken);

            State = EgmState.Stopped;

            _logger.LogInfo($"Stopped {GetType().Name}...");
        }

        /// <inheritdoc />
        public TService GetService<TService>()
            where TService : IHostService
        {
            return _services.GetService<TService>();
        }

        /// <inheritdoc />
        public async Task KeepAlive(CancellationToken cancellationToken)
        {
            var instanceId = ActiveInstance?.InstanceId;
            if (instanceId == null)
            {
                throw new ServerResponseException(ServerResponseCode.InvalidInstanceId);
            }

            await _queue.Send<KeepAlive, KeepAliveResponse>(
                new KeepAlive { InstanceId = instanceId.Value },
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task Connect(IPEndPoint endPoint, CancellationToken cancellationToken)
        {
            var policy = Policy
                .Handle<Exception>(_ => !cancellationToken.IsCancellationRequested)
                .WaitAndRetryAsync(
                    ProtocolConstants.DefaultConnectionRetries,
                    r => TimeSpan.FromSeconds(ConnectionRetryDelay(r)),
                    async (_, rc, r, _) =>
                    {
                        _logger.LogDebug(
                            $"Retrying ({r}) connection to {endPoint} in T-{rc}.");
                        await Task.CompletedTask;
                    });

            await policy.ExecuteAsync(
                async ct =>
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(ConnectionTimeout));
                    using var lts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                    await _transporter.Connect(endPoint, lts.Token);
                },
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task Disconnect(CancellationToken cancellationToken)
        {
            try
            {
                await Unregister(cancellationToken);
            }
            catch (Exception ex)
            {
                // Unconditionally catch all and continue
                _logger.LogError(ex, "Unregister instance failure.");
            }
            finally
            {
                ActiveInstance = null;
            }

            await _transporter.Disconnect(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> Unregister(CancellationToken cancellationToken)
        {
            bool unregistered;

            var instanceId = ActiveInstance?.InstanceId;
            if (instanceId != null)
            {
                var result = await _queue.Send<UnregisterInstance, UnregisterInstanceResponse>(
                    new UnregisterInstance { InstanceId = instanceId.Value },
                    cancellationToken);

                _logger.LogInfo($"Un-registered instance with response code: {result.Response?.ResponseCode}");

                unregistered = result.Response?.ResponseCode == ServerResponseCode.Ok;
            }
            else
            {
                _logger.LogInfo("Already un-registered");

                unregistered = true;
            }

            if (unregistered)
            {
                ActiveInstance = null;
            }

            return unregistered;
        }

        private async Task Start(IStartable startable)
        {
            if (!startable.CanStart())
            {
                _logger.LogWarn($"{startable.GetType().Name} cannot be started.");
                return;
            }

            try
            {
                await startable.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Starting {startable.GetType().Name}.");
                throw;
            }
        }

        private async Task Stop(IStartable startable)
        {
            try
            {
                await startable.Stop();
            }
            catch (Exception ex)
            {
                // Unconditionally catch all and continue
                _logger.LogError(ex, $"Stopping {startable.GetType().Name}.");
            }
        }

        private static int ConnectionRetryDelay(int retry)
        {
            if (retry == 0 || retry == 1)
            {
                return retry;
            }

            return ConnectionRetryDelay(retry - 1) + ConnectionRetryDelay(retry - 2);
        }
    }
}
