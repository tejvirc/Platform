namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Provides access to progressive Id values from protocol
    /// </summary>
    public interface IProtocolProgressiveIdProvider
    {
        /// <summary>
        ///     Overrides progressive levelId with mappings provided by protocol
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="progressiveId"></param>
        /// <param name="levelId"></param>
        void OverrideLevelId(int gameId, int progressiveId, ref int levelId);
    }
}