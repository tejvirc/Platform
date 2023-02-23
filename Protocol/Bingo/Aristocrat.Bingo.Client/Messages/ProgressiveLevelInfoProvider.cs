namespace Aristocrat.Bingo.Client.Messages
{
    using System.Collections.Concurrent;
    using System.Linq;

    /// <summary>
    ///     A provider for getting the current progressive level information sent from the progressive server
    /// </summary>
    public class ProgressiveLevelInfoProvider : IProgressiveLevelInfoProvider
    {
        private readonly ConcurrentDictionary<int, long> _progressiveIdMapping = new();

        /// <inheritdoc />
        public void AddProgressiveLevelInfo(long progressiveLevelId, int sequenceNumber)
        {
            _progressiveIdMapping.AddOrUpdate(sequenceNumber, progressiveLevelId, (_, _) => progressiveLevelId);
        }

        /// <inheritdoc />
        public long GetProgressiveLevelId(int progressiveId)
        {
            // Sequence numbers from the progressive server are 1-based and platform configured progressive Ids are 1-based so adding +1
            if (_progressiveIdMapping.TryGetValue(progressiveId + 1, out var value))
            {
                return value;
            }

            return -1L;
        }

        /// <inheritdoc />
        public int GetProgressiveSequenceNumber(long progressiveLevelId)
        {
            var results = _progressiveIdMapping
                .Where(x => x.Value == progressiveLevelId)
                .Select(x => x.Key).ToList();

            if (results.Count == 0)
            {
                return -1;
            }

            return results[0];
        }

        /// <inheritdoc />
        public void ClearProgressiveLevelInfo()
        {
            _progressiveIdMapping.Clear();
        }
    }
}