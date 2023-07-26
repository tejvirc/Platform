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
        void UpdateReelState(IDictionary<int, ReelLogicalState> updateData);

        void AnimationUpdated(AnimationUpdatedNotification updateData);

        void NotifyReelSynchronized(int reelIndex);

        void NotifyReelSynchronizeStarted(int reelIndex);
    }
}