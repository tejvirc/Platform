namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    ///     The provider for mapping server supplied progressive level Ids to sequence number.
    ///     This class assumes for side bet implementation that a given progressive level id will only have
    ///     a mapping to one sequence number.
    /// </summary>
    public class ProgressiveLevelInfoProvider : IProgressiveLevelInfoProvider
    {
        // For each gameTitleId/denomination store a dictionary of progressiveLevelId to sequence number
        private readonly Dictionary<(int, long), Dictionary<int, long>> _progressiveIdMapping = new();

        /// <inheritdoc />
        public void AddProgressiveLevelInfo(long progressiveLevelId, int sequenceNumber, int gameTitleId, long denomination)
        {
            if (progressiveLevelId < 0)
            {
                throw new ArgumentException($"Invalid parameter {nameof(progressiveLevelId)} with value {progressiveLevelId}");
            }

            if (sequenceNumber <= 0)
            {
                throw new ArgumentException($"Invalid parameter {nameof(sequenceNumber)} with value {sequenceNumber}");
            }

            if (gameTitleId < 0)
            {
                throw new ArgumentException($"Invalid parameter {nameof(gameTitleId)} with value {gameTitleId}");
            }

            if (denomination <= 0)
            {
                throw new ArgumentException($"Invalid parameter {nameof(denomination)} with value {denomination}");
            }

            // Note that sequence numbers from the progressive server are 1-based so adjusting to 0-based used by progressive core and runtime
            if (_progressiveIdMapping.ContainsKey((gameTitleId, denomination)))
            {
                // TODO Don't add the same key more than once. Server is sending duplicate data right now for each title/denom.
                if (!_progressiveIdMapping[(gameTitleId, denomination)].ContainsKey(sequenceNumber - 1))
                {
                    _progressiveIdMapping[(gameTitleId, denomination)].Add(sequenceNumber - 1, progressiveLevelId);
                }
            }
            else
            {
                var d = new Dictionary<int, long> { { sequenceNumber - 1, progressiveLevelId } };
                _progressiveIdMapping.Add((gameTitleId, denomination), d);
            }
        }

        /// <inheritdoc />
        public long GetServerProgressiveLevelId(int sequenceNumber)
        {
            foreach (var pair in _progressiveIdMapping)
            {
                var progressives = pair.Value;
                if (progressives.ContainsKey(sequenceNumber))
                {
                    return progressives[sequenceNumber];
                }
            }

            return -1L;
        }

        /// <inheritdoc />
        public void ClearProgressiveLevelInfo()
        {
            _progressiveIdMapping.Clear();
        }
    }
}