namespace Aristocrat.Monaco.Hardware.EdgeLight.SequenceLib
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Strips;

    internal class IndividualLedRenderer : BaseRenderer<IndividualLedPatternParameters>, IEdgeLightRenderer
    {
        private List<StripData> _strips = new List<StripData>();

        public int Id => GetHashCode();

        public void Setup(IEdgeLightManager edgeLightManager)
        {
            EdgeLightManager = edgeLightManager;
            _strips = Strips.ToList();
        }

        public void Update()
        {
            _strips.ForEach(
                x =>
                {
                    var colors = Parameters.StripUpdateFunction?.Invoke(x.StripId, x.LedCount);
                    if (colors == null)
                    {
                        EdgeLightManager.ClearStripForPriority(x.StripId, Parameters.Priority);
                    }
                    else
                    {
                        EdgeLightManager.SetStripColors(x.StripId, new LedColorBuffer(colors), 0, Parameters.Priority);
                    }
                }
            );
        }

        public void Clear()
        {
            if (EdgeLightManager == null)
            {
                return;
            }

            _strips.ForEach(x => EdgeLightManager.ClearStripForPriority(x.StripId, Parameters.Priority));
            _strips.Clear();
        }
    }
}