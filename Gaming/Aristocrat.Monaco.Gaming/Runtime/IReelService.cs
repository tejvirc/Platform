namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System.Collections.Generic;
    using Client;
    using GdkRuntime.V1;
    using Hardware.Contracts.Reel;

    /// <summary>
    ///     Provides a mechanism to communicate reel information with a runtime client
    /// </summary>
    public interface IReelService : IClientEndpoint
    {
        /// <summary>
        ///     Updates the reel state
        /// </summary>
        /// <param name="updateData">The update data</param>
        void UpdateReelState(IDictionary<int, ReelLogicalState> updateData);

        /// <summary>
        ///     Notifies of an animation update
        /// </summary>
        /// <param name="updateData">The update data</param>
        void NotifyAnimationUpdated(AnimationUpdatedNotification updateData);
        
        /// <summary>
        ///     Notifies of stepper rule being triggered.
        /// </summary>
        /// <param name="reelId">The reel id</param>
        /// <param name="eventId">The event id</param>
        void NotifyStepperRuleTriggered(int reelId, int eventId);

        /// <summary>
        ///     Notifies of reel synchronization status.
        /// </summary>
        /// <param name="reelId">The reel id</param>
        /// <param name="status">The synchronization status</param>
        void NotifyReelSynchronizationStatus(int reelId, SynchronizeStatus status);

        /// <summary>
        ///     Notifies of a reel stopping.
        /// </summary>
        /// <param name="reelId">The reel id</param>
        /// <param name="timeToStop">The time to stop</param>
        void NotifyReelStopping(int reelId, long timeToStop);
    }
}