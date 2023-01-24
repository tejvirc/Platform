namespace Aristocrat.Monaco.Bingo.Services.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Common;
    using Common.Events;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Timer = System.Timers.Timer;

    public class ProgressiveHandler : IProgressiveInfoHandler, IProgressiveUpdateHandler, IDisposable
    {
        private const int MaximumProgressiveUpdateSeconds = 10;
        private const int MonitorPollTimeSeconds = 5;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly ISystemDisableManager _systemDisable;
        private readonly Dictionary<int, long> _progressiveIdMapping = new();
        private readonly Dictionary<long, DateTime> _progressiveUpdateLastTime = new();
        private readonly List<long> _failedProgressiveLevels = new();
        private readonly double _pollingInterval = TimeSpan.FromSeconds(MonitorPollTimeSeconds).TotalMilliseconds;
        private readonly object _lock = new();
        private bool _disposed;
        private Timer _timer;

        public ProgressiveHandler(
            IEventBus eventBus,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            ISystemDisableManager systemDisable)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
        }

        public Task<bool> ProcessProgressiveInfo(ProgressiveInfoMessage info, CancellationToken token)
        {
            Logger.Debug("Received a progressive information message");

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            token.ThrowIfCancellationRequested();

            Logger.Debug($"ResponseCode={info.ResponseCode}, Accepted={info.Accepted}, GameTitle={info.GameTitleId}, AuthToken={info.AuthenticationToken}");

            lock (_lock)
            {
                _progressiveIdMapping.Clear();
                _progressiveUpdateLastTime.Clear();
                Logger.Debug("Progressive Levels:");
                foreach (var progLevel in info.ProgressiveLevels)
                {
                    Logger.Debug(
                        $"SequenceNumber={progLevel.SequenceNumber}, ProgressiveLevel={progLevel.ProgressiveLevel}");
                    _progressiveIdMapping.Add(progLevel.SequenceNumber - 1, progLevel.ProgressiveLevel);
                    _progressiveUpdateLastTime.Add(progLevel.ProgressiveLevel, DateTime.Now);
                }
            }

            Logger.Debug("Meters To Report:");
            foreach (var meter in info.MetersToReport)
            {
                Logger.Debug($"Meter={meter}");
            }

            SetTimer();

            return Task.FromResult(info.ResponseCode == ResponseCode.Ok);
        }

        public Task<bool> ProcessProgressiveUpdate(ProgressiveUpdateMessage update, CancellationToken token)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            token.ThrowIfCancellationRequested();

            Logger.Debug($"Received a progressive update message, ProgLevel={update.ProgressiveLevel}, Amount={update.Amount}");

            lock (_lock)
            {

                if (_progressiveUpdateLastTime.ContainsKey(update.ProgressiveLevel))
                {
                    _progressiveUpdateLastTime[update.ProgressiveLevel] = DateTime.Now;
                }
            }

            var progressiveLevels = _protocolLinkedProgressiveAdapter.ViewConfiguredProgressiveLevels();
            foreach (var progressiveLevel in progressiveLevels)
            {
                if (_progressiveIdMapping.ContainsKey(progressiveLevel.LevelId))
                {
                    var mappedLevelId = _progressiveIdMapping[progressiveLevel.LevelId];
                    if (mappedLevelId == update.ProgressiveLevel)
                    {
                        Logger.Debug($"Found mapping of levelId = {progressiveLevel.LevelId} to progressive level = {mappedLevelId}");

                        var linkedLevel = new LinkedProgressiveLevel()
                        {
                            ProtocolName = ProtocolNames.Bingo,
                            ProgressiveGroupId = progressiveLevel.ProgressivePackId,
                            LevelId = progressiveLevel.LevelId,
                            Amount = update.Amount
                        };

                        Logger.Debug(
                            $"UpdateLinkedProgressiveLevels ProgressiveGroupId={linkedLevel.ProgressiveGroupId}, LevelId={linkedLevel.LevelId}, Amount={linkedLevel.Amount}");

                        _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                            new[] { linkedLevel },
                            ProtocolNames.Bingo);

                        return Task.FromResult(true);
                    }
                }
            }

            Logger.Info($"Ignoring progressive update with unknown progressive level {update.ProgressiveLevel}");
            return Task.FromResult(false);
        }

        public Task<bool> DisableByProgressive(DisableByProgressiveMessage disable, CancellationToken token)
        {
            if (disable == null)
            {
                throw new ArgumentNullException(nameof(disable));
            }

            token.ThrowIfCancellationRequested();

            Logger.Debug("Received a disable by progressive message");

            _systemDisable.Disable(
                BingoConstants.ProgresssiveHostOfflineKey,
                SystemDisablePriority.Normal,
                () => $"{Resources.DisabledByProgressiveHost}");

            return Task.FromResult(true);
        }

        public Task<bool> EnableByProgressive(EnableByProgressiveMessage enable, CancellationToken token)
        {
            if (enable == null)
            {
                throw new ArgumentNullException(nameof(enable));
            }

            token.ThrowIfCancellationRequested();

            Logger.Debug("Received an enable by progressive message");

            _systemDisable.Enable(BingoConstants.ProgresssiveHostOfflineKey);

            return Task.FromResult(true);
        }

        private void SetTimer()
        {
            _timer = new Timer(_pollingInterval);
            _timer.Elapsed += PollProgressiveUpdates;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void PollProgressiveUpdates(object source, ElapsedEventArgs e)
        {
            Logger.Debug("PollProgressiveUpdates checking for progressive update failures");

            var hasAnyProgressiveExceededTimeSpan = false;
            var maximumTimeSpan = TimeSpan.FromSeconds(MaximumProgressiveUpdateSeconds);
            var isHostCurrentlyOffline = _failedProgressiveLevels.Count > 0;

            lock (_lock)
            {
                foreach (var pair in _progressiveUpdateLastTime)
                {
                    if ((DateTime.Now - pair.Value) >= maximumTimeSpan)
                    {
                        if (!_failedProgressiveLevels.Contains(pair.Key))
                        {
                            hasAnyProgressiveExceededTimeSpan = true;
                            Logger.Debug($"Progressive level {pair.Key} failed to update in {MaximumProgressiveUpdateSeconds} seconds");
                            _failedProgressiveLevels.Add(pair.Key);
                        }
                    }
                    else
                    {
                        _failedProgressiveLevels.Remove(pair.Key);
                    }
                }
            }

            if (hasAnyProgressiveExceededTimeSpan)
            {
                _eventBus.Publish(new ProgressiveHostOfflineEvent());
            }
            else if (isHostCurrentlyOffline && _failedProgressiveLevels.Count == 0)
            {
                _eventBus.Publish(new ProgressiveHostOnlineEvent());
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
                _timer?.Dispose();
            }

            _disposed = true;
        }
    }
}