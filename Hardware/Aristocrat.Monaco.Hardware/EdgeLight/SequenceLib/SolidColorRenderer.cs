namespace Aristocrat.Monaco.Hardware.EdgeLight.SequenceLib
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;

    internal class SolidColorRenderer : BaseRenderer<SolidColorPatternParameters>, IEdgeLightRenderer
    {
        private bool _dirty = true;
        private List<StripData> _strips = new List<StripData>();

        public int Id => GetHashCode();

        public void Setup(IEdgeLightManager edgeLightManager)
        {
            EdgeLightManager = edgeLightManager;
            _strips = Strips.ToList();
            _dirty = true;
        }

        public void Update()
        {
            if (!_dirty || EdgeLightManager == null)
            {
                return;
            }

            _strips.ForEach(x => EdgeLightManager.SetStripColor(x.StripId, Parameters.Color, Parameters.Priority));
            _dirty = false;
        }

        public void Clear()
        {
            if (EdgeLightManager == null)
            {
                return;
            }

            _dirty = false;
            _strips.ForEach(x => EdgeLightManager.ClearStripForPriority(x.StripId, Parameters.Priority));
            _strips.Clear();
        }
    }
}