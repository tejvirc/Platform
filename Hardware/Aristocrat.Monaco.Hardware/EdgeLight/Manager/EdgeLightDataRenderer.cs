namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Strips;

    internal sealed class EdgeLightDataRenderer : IEdgeLightRenderer
    {
        private readonly ISharedMemoryManager _sharedMemoryManager;
        private EdgeLightData _edgeLightData;
        private IEdgeLightManager _edgeLightManager;
        private StripCloningHandler _mappings;

        public EdgeLightDataRenderer(ISharedMemoryManager sharedMemoryManager)
        {
            _sharedMemoryManager = sharedMemoryManager;
        }

        public int Id => GetHashCode();

        public void Setup(IEdgeLightManager edgeLightManager)
        {
            _edgeLightManager = edgeLightManager;
            _mappings = new StripCloningHandler(_edgeLightManager);
            _edgeLightData = _edgeLightData ?? new EdgeLightData(_sharedMemoryManager);
            _edgeLightData.SetStrips(_edgeLightManager.ExternalLogicalStrips);
        }

        public void Update()
        {
            if (_edgeLightData == null || _edgeLightManager == null)
            {
                return;
            }

            var gameData = _edgeLightData.GameData;
            if (gameData == null)
            {
                return;
            }

            var logicalStrips = _edgeLightManager.ExternalLogicalStrips;
            var commonStrips = gameData.ControlledStrips.Intersect(logicalStrips.Select(x => x.StripId));
            var gameStripData = commonStrips.Select(
                x => (Id: x,
                    Data: new LedColorBuffer(
                        gameData.LightData[x].Take(
                            logicalStrips.First(s => s.StripId == x).LedCount * LedColorBuffer.BytesPerLed))));

            foreach (var (id, data) in _mappings.Clone(gameStripData))
            {
                _edgeLightManager.SetStripColors(
                    id,
                    new LedColorBuffer(data),
                    0,
                    StripPriority.GamePriority);
            }

            _edgeLightManager.SetBrightnessForPriority(gameData.Brightness, StripPriority.GamePriority);
        }

        public void Clear()
        {
            _edgeLightData = null;
        }
    }
}