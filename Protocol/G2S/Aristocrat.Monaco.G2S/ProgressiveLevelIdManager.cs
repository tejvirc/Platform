namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ProgressiveLevelIdManager
    {
        private Dictionary<string, int> _vertexProgressiveLevelIds = new Dictionary<string, int>();
        private Dictionary<string, int> _monacoProgressiveLevelIds = new Dictionary<string, int>();

        private string ConstructKey(int gameId, int progressiveId, int levelId)
        {
            return $"{gameId}|{progressiveId}|{levelId}";
        }

        /// <inheritdoc />
        public int GetVertexProgressiveLevelId(int gameId, int progressiveId, int levelId)
        {
            string key = ConstructKey(gameId, progressiveId, levelId);
            return _vertexProgressiveLevelIds.TryGetValue(key, out int value) ? value : -1;
        }

        /// <inheritdoc />
        public void SetProgressiveLevelIds(Dictionary<string, int> vertexLevelIds)
        {
            _vertexProgressiveLevelIds = vertexLevelIds;
            if (_vertexProgressiveLevelIds?.Count() > 0)
            {
                foreach (var v in _vertexProgressiveLevelIds)
                {
                    var keyTuple = ReplaceLevelIdInKey(v.Key, v.Value);
                    _monacoProgressiveLevelIds[keyTuple.updatedKey] = keyTuple.oldLevelId;
                }
            }
        }

        /// <inheritdoc />
        public void SetVertexProgressiveLevelId(int gameId, int progressiveId, int levelId, int value)
        {
            string key = ConstructKey(gameId, progressiveId, levelId);
            _vertexProgressiveLevelIds[key] = value;
        }

        /// <inheritdoc />
        public int GetMonacoProgressiveLevelId(int gameId, int progressiveId, int levelId)
        {
            string key = ConstructKey(gameId, progressiveId, levelId);
            return _monacoProgressiveLevelIds.TryGetValue(key, out int value) ? value : -1;
        }

        /// <inheritdoc />
        public void SetMonacoProgressiveLevelId(int gameId, int progressiveId, int levelId, int value)
        {
            string key = ConstructKey(gameId, progressiveId, levelId);
            _monacoProgressiveLevelIds[key] = value;
        }

        /// <inheritdoc />
        public (int gameId, int progressiveId, int levelId) FindKeyByVertexValue(int value)
        {
            var entry = _vertexProgressiveLevelIds.FirstOrDefault(x => x.Value == value);
            if (entry.Key == null) return (-1, -1, -1);

            var parts = entry.Key.Split('|').Select(int.Parse).ToArray();
            return (parts[0], parts[1], parts[2]);
        }

        /// <inheritdoc />
        public (int gameId, int progressiveId, int levelId) FindKeyByMonacoValue(int value)
        {
            var entry = _monacoProgressiveLevelIds.FirstOrDefault(x => x.Value == value);
            if (entry.Key == null) return (-1, -1, -1);

            var parts = entry.Key.Split('|').Select(int.Parse).ToArray();
            return (parts[0], parts[1], parts[2]);
        }

        private (string updatedKey, int oldLevelId) ReplaceLevelIdInKey(string key, int newLevelId)
        {
            string[] keyParts = key.Split('|');
            int oldLevelId = int.Parse(keyParts[2]);
            keyParts[2] = newLevelId.ToString();
            string updatedKey = string.Join("|", keyParts);
            return (updatedKey, oldLevelId);
        }

        /// <inheritdoc />
        public bool VertexContainsLevel(ProgressiveLevel level)
        {
            string key = ConstructKey(level.GameId, level.ProgressiveId, level.LevelId);
            return _vertexProgressiveLevelIds.ContainsKey(key);
        }
    }
}
