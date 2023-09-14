namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using Contracts.Reel;
    using RelmReels.Messages;
    using RelmReels.Messages.Queries;
    using MonacoAnimationPreparedStatus = Contracts.Reel.AnimationPreparedStatus;
    using MonacoLightStatus = Contracts.Reel.LightStatus;
    using MonacoReelStatus = Contracts.Reel.ReelStatus;
    using RelmAnimationPreparedStatus = RelmReels.Messages.AnimationPreparedStatus;
    using RelmLightStatus = RelmReels.Messages.LightStatus;
    using RelmReelStatus = RelmReels.Messages.ReelStatus;

    /// <summary>
    ///     Relm extenstion methods
    /// </summary>
    public static class ExtenstionMethods
    {
        /// <summary>
        ///     Converts a Relm reel status to a Monaco reel status
        /// </summary>
        /// <param name="status">The relm reel status</param>
        /// <returns>The Monaco reel status</returns>
        public static MonacoReelStatus ToReelStatus(this DeviceStatus<RelmReelStatus> status)
        {
            return new MonacoReelStatus
            {
                ReelId = status.Id + 1, // Monaco reel indexes are 1-based, whereas RELM uses 0-based indexing.
                Connected = status.Status != RelmReelStatus.Disconnected,
                ReelTampered = status.Status == RelmReelStatus.TamperingDetected,
                ReelStall = status.Status == RelmReelStatus.Stalled,
                OpticSequenceError = status.Status == RelmReelStatus.OpticSequenceError,
                IdleUnknown = status.Status == RelmReelStatus.IdleUnknown,
                UnknownStop = status.Status == RelmReelStatus.UnknownStopReceived,
            };
        }

        /// <summary>
        ///     Converts a Relm light status to a Monaco light status
        /// </summary>
        /// <param name="status">The relm light status</param>
        /// <returns>The Monaco light status</returns>
        public static  MonacoLightStatus ToLightStatus(this DeviceStatus<RelmLightStatus> status)
        {
            return new MonacoLightStatus(status.Id, status.Status == RelmLightStatus.Failure);
        }

        /// <summary>
        ///     Converts a LightShowAnimationQueueLocation to a AnimationQueueType
        /// </summary>
        /// <param name="queueLocation">The queue location</param>
        /// <returns>The AnimationQueueType value</returns>
        public static AnimationQueueType ToAnimationQueueType(this LightShowAnimationQueueLocation queueLocation)
        {
            return queueLocation switch
            {
                LightShowAnimationQueueLocation.NotInTheQueues => AnimationQueueType.NotInQueues,
                LightShowAnimationQueueLocation.RemovedFromPlayAndWaitQueues => AnimationQueueType.PlayAndWaitQueues,
                LightShowAnimationQueueLocation.RemovedFromPlayBecauseAnimationEnded => AnimationQueueType.AnimationEnded,
                LightShowAnimationQueueLocation.RemovedFromPlayingQueue => AnimationQueueType.PlayingQueue,
                LightShowAnimationQueueLocation.RemovedFromWaitingQueue => AnimationQueueType.WaitingQueue,
                _ => AnimationQueueType.Unknown
            };
        }

        /// <summary>
        ///     Converts a RelmAnimationPreparedStatus to a AnimationPreparedStatus
        /// </summary>
        /// <param name="status">The animation prepared status</param>
        /// <returns>The AnimationPreparedStatus value</returns>
        public static MonacoAnimationPreparedStatus ToAnimationPreparedStatus(this RelmAnimationPreparedStatus status)
        {
            return status switch
            {
                RelmAnimationPreparedStatus.AnimationIsIncompatibleWithTheCurrentReelControllerState => MonacoAnimationPreparedStatus.IncompatibleState,
                RelmAnimationPreparedStatus.AnimationQueueFull => MonacoAnimationPreparedStatus.QueueFull,
                RelmAnimationPreparedStatus.AnimationSuccessfullyPrepared => MonacoAnimationPreparedStatus.Prepared,
                RelmAnimationPreparedStatus.FileCorrupt => MonacoAnimationPreparedStatus.FileCorrupt,
                RelmAnimationPreparedStatus.ShowDoesNotExist => MonacoAnimationPreparedStatus.DoesNotExist,
                _ => MonacoAnimationPreparedStatus.Unknown
            };
        }

        /// <summary>
        ///     Converts a ReelPreparedStatus to a AnimationPreparedStatus
        /// </summary>
        /// <param name="status">The animation prepared status</param>
        /// <returns>The AnimationPreparedStatus value</returns>
        public static MonacoAnimationPreparedStatus ToAnimationPreparedStatus(this ReelPreparedStatus status)
        {
            return status switch
            {
                ReelPreparedStatus.AnimationIsIncompatibleWithTheCurrentControllerState => MonacoAnimationPreparedStatus.IncompatibleState,
                ReelPreparedStatus.AnimationQueueFull => MonacoAnimationPreparedStatus.QueueFull,
                ReelPreparedStatus.AnimationSuccessfullyPrepared => MonacoAnimationPreparedStatus.Prepared,
                ReelPreparedStatus.FileCorrupt => MonacoAnimationPreparedStatus.FileCorrupt,
                ReelPreparedStatus.ShowDoesNotExist => MonacoAnimationPreparedStatus.DoesNotExist,
                _ => MonacoAnimationPreparedStatus.Unknown
            };
        }
    }
}
