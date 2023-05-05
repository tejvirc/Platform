namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IProgressiveLevelIdManager
    {
        /// <summary>
        ///     This method gets the Vertex progressive level ids that are used to handle and compose G2S commands.
        /// </summary>
        /// <param name="gameId">
        ///     The game id associated with the progressive level, as defined in <see cref="IGameDetail" />
        /// </param>
        /// <param name="progressiveId">
        ///     The progressive id (same value for Monaco and Vertex)
        /// </param>
        /// <param name="levelId">
        ///     The internal Monaco level id
        /// </param>
        /// <returns>
        /// The Vertex progressive level id
        /// </returns>
        int GetVertexProgressiveLevelId(int gameId, int progressiveId, int levelId);

        /// <summary>
        ///     This method sets the Vertex progressive level ids that are used to handle and compose G2S commands.
        /// </summary>
        /// <param name="gameId">
        ///     The game id associated with the progressive level, as defined in <see cref="IGameDetail" />
        /// </param>
        /// <param name="progressiveId">
        ///     The progressive id (same value for Monaco and Vertex)
        /// </param>
        /// <param name="levelId">
        ///     The internal Monaco level id
        /// </param>
        /// <param name="value">
        ///     The value of the Vertex level id
        /// </param>
        void SetVertexProgressiveLevelId(int gameId, int progressiveId, int levelId, int value);

        /// <summary>
        ///     This method sets the Vertex and Monaco progressive level ids based on the vertexLevelIds dictionary
        /// </summary>
        /// <param name="vertexLevelIds">
        ///     The Vertex progressive level ids dictionary
        /// </param>
        public void SetProgressiveLevelIds(Dictionary<string, int> vertexLevelIds);

        /// <summary>
        ///     This method gets the Monaco progressive level ids that are used by Monaco's ProgressiveLevelProvider
        /// </summary>
        /// <param name="gameId">
        ///     The game id associated with the progressive level, as defined in <see cref="IGameDetail" />
        /// </param>
        /// <param name="progressiveId">
        ///     The Vertex progressive id configured in the Vertex UI and Monaco progressive setup UI
        /// </param>
        /// <param name="levelId">
        ///     The Vertex level id
        /// </param>
        /// <returns>
        /// The Monaco progressive level id
        /// </returns>
        int GetMonacoProgressiveLevelId(int gameId, int progressiveId, int levelId);

        /// <summary>
        ///     This method sets the Monaco progressive level ids that are used by Monaco's ProgressiveLevelProvider
        /// </summary>
        /// <param name="gameId">
        ///     The game id associated with the progressive level, as defined in <see cref="IGameDetail" />
        /// </param>
        /// <param name="progressiveId">
        ///     The Vertex progressive id configured in the Vertex UI and Monaco progressive setup UI
        /// </param>
        /// <param name="levelId">
        ///     The Vertex level id
        /// </param>
        /// <param name="value">
        ///     The value of the Monaco level id
        /// </param>
        void SetMonacoProgressiveLevelId(int gameId, int progressiveId, int levelId, int value);

        /// <summary>
        ///     This method obtains the key which is composed of "GameID|MonacoProgressiveID|MonacoLevelID"
        /// </summary>
        /// <param name="value">
        ///     The value of the Vertex level id
        /// </param>
        /// <returns>
        /// A tuple of gameId, progressive id, and Monaco level id
        /// </returns>
        (int gameId, int progressiveId, int levelId) FindKeyByVertexValue(int value);

        /// <summary>
        ///     This method obtains the key which is composed of "GameID|MonacoProgressiveID|VertexLevelID"
        /// </summary>
        /// <param name="value">
        ///     The value of the Vertex level id
        /// </param>
        /// <returns>
        /// A tuple of gameId, progressive id, and Vertex level id
        /// </returns>
        (int gameId, int progressiveId, int levelId) FindKeyByMonacoValue(int value);
    }
}
