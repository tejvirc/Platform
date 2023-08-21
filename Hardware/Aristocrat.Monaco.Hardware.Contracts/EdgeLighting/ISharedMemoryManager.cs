namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Shared memory Manager: Interfaces with GDK/game via shared memory.
    /// </summary>
    public interface ISharedMemoryManager : IService
    {
        /// <summary>
        ///     Get the light data from the game
        /// </summary>
        GameEdgelightData GetGameData();

        /// <summary>
        ///     Sets the platform strip count
        /// </summary>
        /// <param name="strips"></param>
        void SetPlatformStripCount(IReadOnlyCollection<StripData> strips = null);
    }
}