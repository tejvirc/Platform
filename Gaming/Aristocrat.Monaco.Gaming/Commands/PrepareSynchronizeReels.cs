namespace Aristocrat.Monaco.Gaming.Commands
{
    using Hardware.Contracts.Reel.ControlData;

    /// <summary>
    ///     The <see cref="PrepareSynchronizeReels"/> class
    /// </summary>
    public class PrepareSynchronizeReels
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="PrepareSynchronizeReels"/> class.
        /// </summary>
        /// <param name="reelSyncData">The reel sync data</param>
        public PrepareSynchronizeReels(ReelSynchronizationData reelSyncData)
        {
            ReelSyncData = reelSyncData;
        }

        /// <summary>
        ///     Gets the sync data per reel
        /// </summary>
        public ReelSynchronizationData ReelSyncData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the synchronize was prepared
        /// </summary>
        public bool Success { get; set; }
    }
}
