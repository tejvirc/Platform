namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    using System.Collections.Generic;

    /// <summary>
    ///     The data required for synchronizing reels.
    /// </summary>
    public class ReelSynchronizationData
    {
        /// <summary>
        ///     The duration to sync reels in milliseconds
        /// </summary>
        public short Duration { get; set; }

        /// <summary>
        ///     The reel to synchronize to
        /// </summary>
        public byte MasterReelIndex { get; set; }

        /// <summary>
        ///     The step to synchronize to
        /// </summary>
        public short MasterReelStep { get; set; }

        /// <summary>
        ///     The sync data per reel
        /// </summary>
        public IEnumerable<ReelSyncStepData> ReelSyncStepData { get; set; }

        /// <summary>
        ///     The type of synchronize
        /// </summary>
        public SynchronizeType SyncType { get; set; }
    }
}
