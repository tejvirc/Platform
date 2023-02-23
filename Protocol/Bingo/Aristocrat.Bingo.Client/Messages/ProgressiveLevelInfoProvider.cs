namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    /// <summary>
    ///     The provider for mapping platform progressive level Ids to progressive server progressive level Ids.
    /// </summary>
    public class ProgressiveLevelInfoProvider : IProgressiveLevelInfoProvider
    {
        private readonly ConcurrentDictionary<int, long> _progressiveIdMapping = new();

        /// <inheritdoc />
        public void AddProgressiveLevelInfo(long progressiveLevelId, int sequenceNumber)
        {
            if (progressiveLevelId < 0)
            {
                throw new ArgumentException($"Invalid parameter {nameof(progressiveLevelId)} with value {progressiveLevelId}");
            }

            if (sequenceNumber <= 0)
            {
                throw new ArgumentException($"Invalid parameter {nameof(sequenceNumber)} with value {sequenceNumber} for ");
            }

            // Sequence numbers from the progressive server are 1-based so adjusting to 0-based
            _progressiveIdMapping.AddOrUpdate(sequenceNumber - 1, progressiveLevelId, (_, _) => progressiveLevelId);
        }

        /// <inheritdoc />
        public long GetServerProgressiveLevelId(int progressiveLevelId)
        {
            if (_progressiveIdMapping.TryGetValue(progressiveLevelId, out var value))
            {
                return value;
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