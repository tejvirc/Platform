namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Application.Contracts;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Meters;

    public class ProgressiveLevelManager : IProgressiveLevelManager
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProgressiveMeterManager _progressiveMeters;
        private readonly Dictionary<string, LinkedProgressiveLevel> _pendingUpdates;
        private readonly Timer _updateTimer;
        private bool _isTimerRunning = false;
        private readonly object _lock = new object();
        private const int FlushInterval = 5; // seconds

        public ProgressiveLevelManager(IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter, IProgressiveMeterManager progressiveMeters)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _progressiveMeters = progressiveMeters ?? throw new ArgumentNullException(nameof(progressiveMeters));
            _pendingUpdates = new Dictionary<string, LinkedProgressiveLevel>();
            _updateTimer = new Timer(FlushPendingUpdates, null, Timeout.Infinite, Timeout.Infinite);
        }

        public IEnumerable<simpleMeter> GetProgressiveLevelMeters(int levelDeviceId, params string[] includedMeters)
        {
            return MeterMap.ProgressiveMeters
                .Where(m => includedMeters != null && includedMeters.Any(i => i == m.Value))
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _progressiveMeters.IsMeterProvided(levelDeviceId, meter.Value)
                            ? _progressiveMeters.GetMeter(levelDeviceId, meter.Value).Lifetime
                            : 0
                    });
        }

        public LinkedProgressiveLevel UpdateLinkedProgressiveLevels(
            int progId,
            int levelId,
            long valueInCents,
            long progValueSequence,
            string progValueText,
            bool initialize = false)
        {
            var linkedLevel = CreateLinkedProgressiveLevel(progId, levelId, valueInCents, progValueSequence, progValueText);

            lock (_lock)
            {
                if (_pendingUpdates.TryGetValue(linkedLevel.LevelName, out var existingUpdate))
                {
                    // If an update is already pending for this level, update its values
                    existingUpdate.Amount = linkedLevel.Amount;
                    existingUpdate.WagerCredits = linkedLevel.WagerCredits;
                    existingUpdate.Expiration = linkedLevel.Expiration;
                    existingUpdate.ProgressiveValueSequence = linkedLevel.ProgressiveValueSequence;
                    existingUpdate.ProgressiveValueText = linkedLevel.ProgressiveValueText;
                }
                else
                {
                    // Add the new update to the dictionary
                    _pendingUpdates[linkedLevel.LevelName] = linkedLevel;
                }

                // Start the timer for the first cached update
                if (_pendingUpdates.Count > 0 && !_isTimerRunning)
                {
                    _updateTimer.Change(TimeSpan.FromSeconds(FlushInterval), TimeSpan.FromSeconds(FlushInterval));
                    _isTimerRunning = true;
                }
            }

            if (!initialize)
            {
                return linkedLevel;
            }

            if (!_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Any(l => l.LevelName.Equals(linkedLevel.LevelName)))
            {
                FlushPendingUpdates(null);
            }

            return linkedLevel;
        }

        private void FlushPendingUpdates(object state)
        {
            List<LinkedProgressiveLevel> pendingUpdates;

            lock (_lock)
            {
                pendingUpdates = _pendingUpdates.Values.ToList();
                _pendingUpdates.Clear();
                _updateTimer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer until the next update
                _isTimerRunning = false;
            }

            if (pendingUpdates.Count > 0)
            {
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                    pendingUpdates.ToArray(),
                    ProtocolNames.G2S);
            }
        }

        private LinkedProgressiveLevel CreateLinkedProgressiveLevel(
            int progId,
            int levelId,
            long valueInCents,
            long progValueSequence,
            string progValueText)
        {
            return new LinkedProgressiveLevel
            {
                ProtocolName = ProtocolNames.G2S,
                ProgressiveGroupId = progId,
                LevelId = levelId,
                Amount = valueInCents,
                Expiration = DateTime.UtcNow.AddDays(365),
                CurrentErrorStatus = ProgressiveErrors.None,
                ProgressiveValueSequence = progValueSequence,
                ProgressiveValueText = progValueText
            };
        }
    }
}
