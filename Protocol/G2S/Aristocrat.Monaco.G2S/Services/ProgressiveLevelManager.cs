namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Humanizer.Localisation.NumberToWords;
    using Meters;

    public class ProgressiveLevelManager : IProgressiveLevelManager, IDisposable
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProgressiveMeterManager _progressiveMeters;
        private readonly Dictionary<string, LinkedProgressiveLevel> _pendingUpdates;
        private readonly Timer _updateTimer;
        private bool _isTimerRunning = false;
        private readonly object _lock = new object();
        private const int FlushInterval = 5; // seconds

        private readonly string[] basicMetersStandard = new[]
        {
            ProgressiveMeters.LinkedProgressiveWageredAmount,
            ProgressiveMeters.LinkedProgressivePlayedCount
        };
        private readonly string[] basicMetersBulk = new[]
        {
            ProgressiveMeters.LinkedProgressiveWageredAmount,
            ProgressiveMeters.LinkedProgressiveWageredAmountWithAnte,
            ProgressiveMeters.LinkedProgressivePlayedCount
        };

        public ProgressiveLevelManager(IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter, IProgressiveMeterManager progressiveMeters)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _progressiveMeters = progressiveMeters ?? throw new ArgumentNullException(nameof(progressiveMeters));
            _pendingUpdates = new Dictionary<string, LinkedProgressiveLevel>();
            _updateTimer = new Timer(FlushPendingUpdates, null, Timeout.Infinite, Timeout.Infinite);
        }

        public IEnumerable<deviceMeters> GetProgressiveDeviceMeters(int deviceId)
        {
            var linkedLevels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Where(ll => ll.ProgressiveGroupId == deviceId && ll.ProtocolName == ProtocolNames.G2S).ToList();

            var anyBulkLevels = linkedLevels.Any(ll => ll.FlavorType == FlavorType.BulkContribution);

            if (anyBulkLevels)
            {
                return BuildBulkLevelMeters(deviceId, linkedLevels);
            }
            else
            {
                return new[]
                {
                    new deviceMeters
                    {
                        deviceClass = DeviceClass.G2S_progressive,
                        deviceId = deviceId,
                        simpleMeter = GetSimpleProgressiveLevelMeters(linkedLevels.First().LevelName, basicMetersStandard).ToArray()
                    }
                };
            }
        }

        private IEnumerable<deviceMeters> BuildBulkLevelMeters(int deviceId, IList<IViewableLinkedProgressiveLevel> linkedLevels)
        {
            //The basic meters are evenly incremented across all levels for the progressive device
            var basicMeters = GetSimpleProgressiveLevelMeters(linkedLevels.First().LevelName, basicMetersBulk);

            //bulk meters are level specific and must be built for each applicable level. 
            var bulkMeters = GetSimpleBulkProgressiveLevelMeters(linkedLevels);

            var complexMeters = GetComplexProgressiveLevelMeters(linkedLevels);

            return new[]
            {
                new deviceMeters
                {
                    deviceClass = DeviceClass.G2S_progressive,
                    deviceId = deviceId,
                    simpleMeter = basicMeters.Concat(bulkMeters).ToArray(),
                    complexMeter = complexMeters.ToArray()
                }
            };
        }

        private IEnumerable<simpleMeter> GetSimpleProgressiveLevelMeters(string linkedLevelName, params string[] includedMeters)
        {
            return MeterMap.ProgressiveMeters
                .Where(m => includedMeters != null && includedMeters.Any(i => i == m.Value))
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase) ||
                                    meter.Key.StartsWith("ATI_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _progressiveMeters.IsMeterProvided(linkedLevelName, meter.Value)
                            ? _progressiveMeters.GetMeter(linkedLevelName, meter.Value).Lifetime
                            : 0
                    });
        }

        private IEnumerable<simpleMeter> GetSimpleBulkProgressiveLevelMeters(
            IEnumerable<IViewableLinkedProgressiveLevel> linkedLevels)
        {
            return from linkedLevel in linkedLevels
                where linkedLevel.FlavorType == FlavorType.BulkContribution
                join meter in MeterMap.BulkContributionMeters on linkedLevel.LevelId equals meter.Key
                select new simpleMeter
                {
                    meterName = meter.Value,
                    meterValue = _progressiveMeters.IsMeterProvided(
                        linkedLevel.LevelName,
                        ProgressiveMeters.LinkedProgressiveBulkContribution)
                        ? _progressiveMeters.GetMeter(
                            linkedLevel.LevelName,
                            ProgressiveMeters.LinkedProgressiveBulkContribution).Lifetime
                        : 0
                };
        }

        private IEnumerable<complexMeter> GetComplexProgressiveLevelMeters(IList<IViewableLinkedProgressiveLevel> linkedLevels, params string[] includedMeters)
        {
            return new List<complexMeter>();
        }

        public LinkedProgressiveLevel UpdateLinkedProgressiveLevels(
            int progId,
            int levelId,
            long valueInCents,
            long progValueSequence,
            string progValueText,
            FlavorType flavorType,
            bool initialize = false,
            string commonLevelName = null)
        {
            if (initialize && string.IsNullOrWhiteSpace(commonLevelName))
            {
                throw new ArgumentNullException(
                    nameof(commonLevelName),
                    @"CommonLevelName required when initializing a new linked level");
            }

            var linkedLevel = CreateLinkedProgressiveLevel(progId, levelId, commonLevelName, valueInCents, progValueSequence, progValueText, flavorType);

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
                FlushPendingUpdates();
            }

            return linkedLevel;
        }

        private void FlushPendingUpdates(object state = null)
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

        private static LinkedProgressiveLevel CreateLinkedProgressiveLevel(
            int progId,
            int levelId,
            string commonLevelName,
            long valueInCents,
            long progValueSequence,
            string progValueText,
            FlavorType flavorType)
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
                ProgressiveValueText = progValueText,
                FlavorType = flavorType,
                CommonLevelName = commonLevelName
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateTimer?.Dispose();
            }
        }
    }
}
