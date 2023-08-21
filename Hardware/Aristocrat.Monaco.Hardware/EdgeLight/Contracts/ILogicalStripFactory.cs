namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    using System.Collections.Generic;

    public interface ILogicalStripFactory
    {
        IList<IStrip> GetLogicalStrips(IReadOnlyCollection<IStrip> physicalStrips);
    }
}