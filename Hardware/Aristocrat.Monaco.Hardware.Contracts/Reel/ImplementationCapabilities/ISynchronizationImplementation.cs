namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ImplementationCapabilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ControlData;
    using Events;

    /// <summary>
    ///     The reel controller synchronization capability of an implementation
    /// </summary>
    public interface ISynchronizationImplementation : IReelImplementationCapability
    {
        /// <summary>
        ///     The event that occurs when the reel controller starts reel synchronization
        /// </summary>
        event EventHandler<ReelSynchronizationEventArgs> SynchronizationStarted;
        
        /// <summary>
        ///     The event that occurs when the reel controller completes reel synchronization
        /// </summary>
        event EventHandler<ReelSynchronizationEventArgs> SynchronizationCompleted;

        /// <summary>
        ///     Instructs the reel controller to synchronize the reels
        /// </summary>
        /// <param name="syncData">The synchronization data.</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task<bool> Synchronize(ReelSynchronizationData syncData, CancellationToken token = default);
    }
}
