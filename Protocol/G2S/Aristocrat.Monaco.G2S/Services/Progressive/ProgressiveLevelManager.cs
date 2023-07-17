namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public ProgressiveLevelManager(IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter, IProgressiveMeterManager progressiveMeters)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _progressiveMeters = progressiveMeters ?? throw new ArgumentNullException(nameof(progressiveMeters));
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

        /// <inheritdoc />
        public LinkedProgressiveLevel UpdateLinkedProgressiveLevels(
        int progId,
        int levelId,
        long valueInCents,
        long progValueSequence,
        string progValueText,
        bool initialize = false)
        {
            var linkedLevel = LinkedProgressiveLevel(progId, levelId, valueInCents, progValueSequence, progValueText);

            if (!initialize || !_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Any(l => l.LevelName.Equals(linkedLevel.LevelName)))
            {
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                    new[] { linkedLevel },
                    ProtocolNames.G2S);
            }

            return linkedLevel;
        }

        private static LinkedProgressiveLevel LinkedProgressiveLevel(
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