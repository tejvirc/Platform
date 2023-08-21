namespace Aristocrat.Monaco.Hardware.EdgeLight.SequenceLib
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;

    internal abstract class BaseRenderer<TParameterType> where TParameterType : PatternParameters
    {
        protected IEdgeLightManager EdgeLightManager { get; set; }
        public TParameterType Parameters { get; set; }

        protected IEnumerable<StripData> Strips =>
            EdgeLightManager?.LogicalStrips.Where(
                x => ReferenceEquals(Parameters.Strips, PatternParameters.AllStrips) ||
                     Parameters.Strips.Contains(x.StripId)).OrderBy(x => x.StripId)
            ?? new List<StripData>().OrderBy(x => x.StripId);
    }
}