﻿namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Strips;

    internal sealed class EdgeLightDataRenderer : IEdgeLightRenderer
    {
        private EdgeLightData _edgeLightData;
        private IEdgeLightManager _edgeLightManager;
        private StripCloningHandler _mappings;

        public int Id => GetHashCode();

        public void Setup(IEdgeLightManager edgeLightManager)
        {
            _edgeLightManager = edgeLightManager;
            _mappings = new StripCloningHandler(_edgeLightManager);
            _edgeLightData = _edgeLightData ?? new EdgeLightData();
            _edgeLightData.SetStrips(_edgeLightManager.LogicalStrips);
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

            var logicalStrips = _edgeLightManager.LogicalStrips;
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