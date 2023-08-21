namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    /// Provides a snapshot of meters for an active game round.
    /// </summary>
    public interface IGameRoundMeterSnapshotProvider
    {
        /// <summary>
        /// Creates a meter snapshot with the specified PlayState.
        /// </summary>
        GameRoundMeterSnapshot GetSnapshot(PlayState playState);
    }
}
