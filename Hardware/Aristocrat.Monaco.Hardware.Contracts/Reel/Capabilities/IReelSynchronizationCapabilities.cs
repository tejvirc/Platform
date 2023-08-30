namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities
{
    using System.Threading;
    using System.Threading.Tasks;
    using ControlData;

    /// <summary>
    ///     The public interface for reel controller synchronization capabilities
    /// </summary>
    public interface IReelSynchronizationCapabilities : IReelControllerCapability
    {
        /// <summary>
        ///     Instructs the reel controller to synchronize the reels
        /// </summary>
        /// <param name="syncData">The synchronization data.</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task<bool> Synchronize(ReelSynchronizationData syncData, CancellationToken token = default);
    }
}
