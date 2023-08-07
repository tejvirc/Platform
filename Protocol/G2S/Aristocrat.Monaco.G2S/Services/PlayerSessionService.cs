namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Common.Events;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Session;
    using Handlers;
    using Handlers.Player;
    using Kernel;
    using log4net;

    public class PlayerSessionService : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private readonly IG2SEgm _egm;
        private readonly IGameMeterManager _meters;
        private readonly IPlayerService _players;

        private readonly object _retryLock = new object();

        private ActionBlock<IPlayerSessionLog> _messageProcessor;
        private CancellationTokenSource _cancelProcessing;
        private volatile bool _canSend;

        private IPlayerSessionLog _currentLog;
        private Timer _sessionStartRetry;

        private bool _disposed;

        public PlayerSessionService(IG2SEgm egm, IPlayerService players, IPlayerSessionHistory sessions, IEventBus bus, IGameMeterManager meters)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));

            _bus.Subscribe<SessionStartedEvent>(this, Handle);
            _bus.Subscribe<SessionEndedEvent>(this, Handle);
            _bus.Subscribe<CommunicationsStateChangedEvent>(this, evt =>
            {
                var playerDevice = _egm.GetDevice<IPlayerDevice>();
                if (playerDevice?.IsOwner(evt.HostId) == true)
                    _canSend = evt.Online;
            });

            _sessionStartRetry = new Timer(RetrySessionStart, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            _cancelProcessing = new CancellationTokenSource();

            _messageProcessor = new ActionBlock<IPlayerSessionLog>(
                async request =>
                {
                    while (!_canSend && !_cancelProcessing.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), _cancelProcessing.Token);
                    }

                    _cancelProcessing.Token.ThrowIfCancellationRequested();

                    while (!_cancelProcessing.IsCancellationRequested && !HandleSessionEnd(request))
                    {
                        var playerDevice = _egm.GetDevice<IPlayerDevice>();

                        await Task.Delay(playerDevice?.Queue.SessionTimeout ?? Constants.DefaultTimeout, _cancelProcessing.Token);
                    }
                });

            var history = sessions.GetHistory()
                .Where(h => h.PlayerSessionState == PlayerSessionState.SessionCommit)
                .OrderBy(h => h.TransactionId).ToList();
            foreach (var log in history)
            {
                Logger.Debug($"Queueing session to be acknowledged [{log.TransactionId}]");
                _messageProcessor.Post(log);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);

                _sessionStartRetry.Dispose();

                _cancelProcessing.Cancel(false);
                _messageProcessor.Complete();
                _messageProcessor.Completion.Wait();

                _cancelProcessing.Dispose();
            }

            _sessionStartRetry = null;
            _messageProcessor = null;
            _cancelProcessing = null;

            _disposed = true;
        }

        private void Handle(SessionStartedEvent evt)
        {
            _sessionStartRetry.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            HandleSessionStart(evt.Log);
        }

        private void Handle(SessionEndedEvent evt)
        {
            _messageProcessor.Post(evt.Log);
        }

        private void HandleSessionStart(IPlayerSessionLog log)
        {
            if (log.PlayerSessionState != PlayerSessionState.SessionOpen)
            {
                return;
            }

            lock (_retryLock)
            {
                var playerDevice = _egm.GetDevice<IPlayerDevice>();
                if (playerDevice == null)
                {
                    return;
                }

                var (timedOut, response) = playerDevice.StartSession(
                    log.TransactionId,
                    log.IdReaderType.ToIdReaderType(),
                    log.IdNumber,
                    log.PlayerId,
                    log.StartDateTime);

                if (response == null)
                {
                    _currentLog = log;

                    _sessionStartRetry.Change(
                        timedOut ? TimeSpan.Zero : playerDevice.Queue.SessionTimeout,
                        Timeout.InfiniteTimeSpan);

                    return;
                }

                GenericOverrideParameters overrideParams = null;
                if (response.overrideId != 0)
                {
                    overrideParams = new GenericOverrideParameters(
                        response.playerTarget,
                        response.playerIncrement,
                        response.playerAward,
                        response.playerStart,
                        response.playerEnd,
                        response.overrideId);
                }

                _players.SetSessionParameters(log.TransactionId, response.pointBalance, response.hostCarryOver, overrideParams);

                _currentLog = null;
            }
        }

        private bool HandleSessionEnd(IPlayerSessionLog log)
        {
            var device = _egm.GetDevice<IPlayerDevice>();
            if (device == null)
            {
                return false;
            }

            var response = device.SendMeterDelta
                ? device.EndSession(
                    log.ToPlayerSessionEndExt(
                        device,
                        device.SubscribedMeters.Expand(_egm.Devices),
                        (id, meterName) => _meters.GetMeterName(id, meterName)))
                : device.EndSession(log.ToPlayerSessionEnd(device));

            if (response != null)
            {
                Logger.Debug($"Player session acknowledged [{log.TransactionId}]");

                _players.CommitSession(response.Value);
                return true;
            }

            Logger.Debug($"Failed to ack player session [{log.TransactionId}]");

            return false;
        }

        private void RetrySessionStart(object state)
        {
            if (_currentLog != null)
            {
                HandleSessionStart(_currentLog);
            }
        }
    }
}
