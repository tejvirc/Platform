namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.EdgeLighting;

    /// <summary>
    ///     Interfaces with shared memory to transfer data.
    /// </summary>
    internal class EdgeLightData
    {
        private readonly ISharedMemoryManager _sharedMemoryManager;

        public EdgeLightData(ISharedMemoryManager sharedMemory)
        {
            _sharedMemoryManager = sharedMemory ?? throw new ArgumentNullException(nameof(sharedMemory));
        }

        public GameEdgelightData GameData => _sharedMemoryManager.GetGameData();

        public void SetStrips(IReadOnlyCollection<StripData> strips)
        {
            _sharedMemoryManager.SetPlatformStripCount(strips);
        }
    }
}