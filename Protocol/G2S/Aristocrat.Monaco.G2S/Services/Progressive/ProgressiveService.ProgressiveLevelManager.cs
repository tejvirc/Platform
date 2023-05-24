namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.G2S.Meters;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Protocol.v21;

    public partial class ProgressiveService : IProgressiveLevelManager, IProtocolProgressiveIdProvider
    {
        /// <inheritdoc />
        public ProgressiveLevelIdManager LevelIds { get; } = new ProgressiveLevelIdManager();

        /// <inheritdoc />
        public Dictionary<string, ProgressiveValue> ProgressiveValues { get; set; } = new Dictionary<string, ProgressiveValue>();

        /// <inheritdoc />
        public IEnumerable<simpleMeter> GetProgressiveLevelMeters(int deviceId, params string[] includedMeters)
        {
            return MeterMap.ProgressiveMeters
                .Where(m => { return includedMeters != null && includedMeters.Any(i => i == m.Value); })
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _progressiveMeters.IsMeterProvided(deviceId, meter.Value)
                            ? _progressiveMeters.GetMeter(deviceId, meter.Value).Lifetime
                            : 0
                    });
        }

        /// <inheritdoc />
        public LinkedProgressiveLevel UpdateLinkedProgressiveLevels(
        int progId,
        int levelId,
        long valueInCents,
        bool initialize = false)
        {
            var linkedLevel = LinkedProgressiveLevel(progId, levelId, valueInCents);

            if (!initialize || !_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Any(l => l.LevelName.Equals(linkedLevel.LevelName)))
            {
                _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                    new[] { linkedLevel },
                    ProtocolNames.G2S);
            }

            return linkedLevel;
        }

        private static LinkedProgressiveLevel LinkedProgressiveLevel(int progId, int levelId, long valueInCents)
        {
            var linkedLevel = new LinkedProgressiveLevel
            {
                ProtocolName = ProtocolNames.G2S,
                ProgressiveGroupId = progId,
                LevelId = levelId,
                Amount = valueInCents,
                Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
                CurrentErrorStatus = ProgressiveErrors.None
            };

            return linkedLevel;
        }

        /// <inheritdoc />
        public void OverrideLevelId(int gameId, int progressiveId, ref int levelId)
        {
            if (!_g2sProgressivesEnabled)
            {
                return;
            }

            var vertexLevelId = LevelIds.GetVertexProgressiveLevelId(gameId, progressiveId, levelId);

            if (vertexLevelId != -1)
            {
                levelId = vertexLevelId;
            }
        }

    }
}
