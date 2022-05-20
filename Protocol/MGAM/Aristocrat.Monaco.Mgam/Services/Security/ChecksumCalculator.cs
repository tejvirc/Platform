namespace Aristocrat.Monaco.Mgam.Services.Security
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Application.Contracts.Authentication;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Common.Events;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using Lockup;
    using Monaco.Common;
    using Monaco.Common.Cryptography;
    using Notification;
    using Protocol.Common.Storage.Entity;
    using Checksum = Common.Data.Models.Checksum;

    /// <summary>
    ///     Used to calculate the checksum.
    /// </summary>
    public sealed class ChecksumCalculator : IChecksumCalculator, IService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;

        private readonly ActionBlock<Request> _checksumProcessor;

        private readonly ConcurrentDictionary<int, CancellationTokenSource> _requests = new ConcurrentDictionary<int, CancellationTokenSource>();

        private CancellationTokenSource _shutdown = new CancellationTokenSource();

        private ManualResetEvent _connected = new ManualResetEvent(false);

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChecksumCalculator" /> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="commandFactory">Command Factory.</param>
        /// <param name="unitOfWorkFactory">Unit of work factory.</param>
        /// <param name="authenticationService">Authentication Service.</param>
        /// <param name="lockup"><see cref="ILockup"/>.</param>
        /// <param name="notificationLift">Host notification lift.</param>
        /// <param name="eventBus"><see cref="IEventBus" />.</param>
        /// <param name="gameProvider"><see cref="IGameProvider"/></param>
        public ChecksumCalculator(
            ILogger<ChecksumCalculator> logger,
            ICommandHandlerFactory commandFactory,
            IUnitOfWorkFactory unitOfWorkFactory,
            IAuthenticationService authenticationService,
            ILockup lockup,
            INotificationLift notificationLift,
            IEventBus eventBus,
            IGameProvider gameProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));

            _checksumProcessor = new ActionBlock<Request>(
                async request =>
                {
                    if (_shutdown.IsCancellationRequested)
                    {
                        return;
                    }

                    bool retry;

                    var cts = new CancellationTokenSource();

                    try
                    {
                        _requests.TryAdd(request.Seed, cts);

                        retry = await ProcessRequest(request, cts.Token) == FailureAction.Retry;
                    }
                    finally
                    {
                        _requests.TryRemove(request.Seed, out _);
                        cts.Dispose();
                    }

                    if (retry && !_shutdown.IsCancellationRequested)
                    {
                        _logger.LogInfo($"Retrying checksum request, Seed={request.Seed}, Id={request.Id}");
                        await Calculate(request.Seed);
                    }
                });

            _checksumProcessor.Completion.ContinueWith(
                t =>
                {
                    if (!t.IsFaulted)
                    {
                        return;
                    }

                    t.Exception?.Handle(
                        ex =>
                        {
                            _logger.LogError(ex, "Checksum processor failure");
                            return true;
                        });
                });

            SubscribeToEvents();
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IChecksumCalculator) };

        /// <inheritdoc />
        public void Initialize()
        {
            _logger.LogInfo($"{nameof(ChecksumCalculator)} initialized");
        }

        /// <inheritdoc />
        public void Stop()
        {
            _logger.LogInfo($"{nameof(ChecksumCalculator)} stopping");

            _shutdown.Cancel();

            _checksumProcessor.Complete();
            _checksumProcessor.Completion.Wait();

            _logger.LogInfo($"{nameof(ChecksumCalculator)} stopped");
        }

        /// <inheritdoc />
        public async Task Calculate(int? seed = null)
        {
            Checksum checksum;

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var checksumRepository = unitOfWork.Repository<Checksum>();

                if (seed == null)
                {
                    checksum = checksumRepository.Queryable().FirstOrDefault();
                }
                else
                {
                    checksum = checksumRepository.Queryable().FirstOrDefault(c => c.Seed == seed.Value) ??
                               await PersistChecksum(seed.Value);
                }
            }

            if (checksum == null || _requests.ContainsKey(checksum.Seed))
            {
                return;
            }

            Abort();

            _logger.LogInfo($"Posting checksum request, Seed={checksum.Seed}, Id={checksum.Id}");

            _checksumProcessor.Post(new Request(checksum.Id, checksum.Seed));
        }

        /// <inheritdoc />
        public bool ValidateFile(string fileName, uint checksum)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                var sha256Hash = HashAlgorithm.Create(AlgorithmType.Sha256.ToString().ToUpperInvariant());
                if (sha256Hash == null)
                {
                    return false;
                }

                var result = sha256Hash.ComputeHash(fileStream);

                var builder = new StringBuilder();
                foreach (var toChar in result)
                {
                    builder.Append(toChar.ToString("x2"));
                }

                return Crc32Algorithm.Append(0, Encoding.UTF8.GetBytes(builder.ToString().ToUpper())) == checksum;
            }
        }

        /// <inheritdoc />
        public bool CheckSignature(string signature)
        {
            return _authenticationService.CalculateRomHash(AlgorithmType.Sha256).Equals(signature);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_shutdown != null)
            {
                _shutdown.Dispose();
            }

            if (_connected != null)
            {
                _connected.Dispose();
            }

            _shutdown = null;
            _connected = null;

            _disposed = true;
        }

        private async Task<FailureAction> ProcessRequest(Request request, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(
                async () =>
                {
                    _logger.LogInfo($"Aborting checksum request, Seed={request.Seed}, Id={request.Id}");
                    await DeleteChecksum(request.Id);
                }))
            {
                int result;

                try
                {
                    result = _authenticationService.CalculateRomCrc32(request.Seed);
                }
                catch (Exception ex)
                {
                    await Failed(
                        new InvalidOperationException($"Compute checksum for seed {request.Seed} failed", ex),
                        request,
                        0);
                    return FailureAction.None;
                }

                try
                {
                    using (var lts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token))
                    {
                        await _connected.AsTask(lts.Token);

                        _logger.LogInfo(
                            $"Sending checksum request, Seed={request.Seed}, Result={result}, Id={request.Id}");

                        await _commandFactory.Execute(
                            new Commands.Checksum { ChecksumValue = result, CancellationToken = cancellationToken });

                        await DeleteChecksum(request.Id);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Request aborted nothing to do
                }
                catch (AggregateException ex) when (TaskCancelled(ex))
                {
                    // Request aborted nothing to do
                }
                catch (ServerResponseException ex)
                {
                    if (ex.ResponseCode == ServerResponseCode.ChecksumFailure)
                    {
                        await Failed(ex, request, result);
                    }
                    else
                    {
                        _logger.LogError(
                            ex,
                            $"Sending checksum failed, Seed={request.Seed}, Result={result}, Id={request.Id}");
                        return FailureAction.Retry;
                    }
                }

                return FailureAction.None;
            }
        }

        private void Abort()
        {
            var requests = _requests.Values.ToList();
            _requests.Clear();

            foreach (var r in requests)
            {
                r.Cancel();
            }
        }

        private async Task Failed(Exception exception, Request request, int result)
        {
            _logger.LogError(exception, $"Checksum request failed, Seed={request.Seed}, Result={result}, Id={request.Id}");

            await DeleteChecksum(request.Id);

            _lockup.LockupForEmployeeCard(
                $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByHost)} Checksum Failed");

            var game = _gameProvider.GetEnabledGames().FirstOrDefault();
            await _notificationLift.Notify(NotificationCode.LockedChecksumFailed, $"{game?.ThemeId} {game?.Version}");
        }

        private Task<Checksum> PersistChecksum(int seed)
        {
            _logger.LogInfo($"Persisting checksum, Seed={seed}");

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var checksum = new Checksum { Seed = seed };

                unitOfWork.Repository<Checksum>().Add(checksum);

                unitOfWork.SaveChanges();

                return Task.FromResult(checksum);
            }
        }

        private Task DeleteChecksum(long id)
        {
            _logger.LogInfo($"Deleting checksum, Id={id}");

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                unitOfWork.Repository<Checksum>().Delete(id);

                unitOfWork.SaveChanges();

                return Task.CompletedTask;
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<HostOfflineEvent>(this, _ => _connected.Reset());
            _eventBus.Subscribe<AttributesUpdatedEvent>(this, Handle);
        }

        private async Task Handle(AttributesUpdatedEvent evt, CancellationToken cancellationToken)
        {
            _connected.Set();

            await Calculate();
        }

        private static bool TaskCancelled(AggregateException ex)
        {
            return ex?.InnerExceptions.Any(e => e.GetType() == typeof(TaskCanceledException)) ?? false;
        }

        private readonly struct Request
        {
            public Request(long id, int seed)
            {
                Id = id;
                Seed = seed;
            }

            public long Id { get; }

            public int Seed { get; }
        }

        private enum FailureAction
        {
            None,

            Retry
        }
    }
}
