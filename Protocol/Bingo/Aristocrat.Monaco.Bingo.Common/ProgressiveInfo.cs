namespace Aristocrat.Monaco.Bingo.Common
{
    /// <summary>
    ///     Defines a progressive jackpot.
    /// </summary>
    public readonly struct ProgressiveInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveInfo"/> class.
        /// </summary>
        /// <param name="packName">Progressive pack name.</param>
        /// <param name="progId">Progressive identifier.</param>
        /// <param name="levelId">Progressive level identifier.</param>
        /// <param name="levelName">Progressive level name.</param>
        /// <param name="poolName">Progressive jackpot name.</param>
        public ProgressiveInfo(string packName, int progId, int levelId, string levelName, string poolName)
        {
            PackName = packName;
            ProgId = progId;
            LevelId = levelId;
            LevelName = levelName;
            PoolName = poolName;
        }

        /// <summary>
        ///     Gets the progressive pack name.
        /// </summary>
        public string PackName { get; }

        /// <summary>
        ///     Gets the progressive identifier.
        /// </summary>
        public int ProgId { get; }

        /// <summary>
        ///     Gets the progressive level identifier.
        /// </summary>
        public int LevelId { get; }

        /// <summary>
        ///     Gets the progressive level name.
        /// </summary>
        public string LevelName { get; }

        /// <summary>
        ///     Gets the progressive jackpot name.
        /// </summary>
        public string PoolName { get; }
    }
}
