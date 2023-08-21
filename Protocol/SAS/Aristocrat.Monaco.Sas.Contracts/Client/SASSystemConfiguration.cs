namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System.Collections.Generic;

    /// <summary>
    /// Definition of the SasSystemConfiguration class.
    /// </summary>
    public class SasSystemConfiguration
    {
        /// <summary>
        /// Gets or sets a list of Sas host configurations, one per port
        /// </summary>
        public IList<SasHostConfiguration> SasHostConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the Sas configuration from file
        /// </summary>
        public SasConfiguration SasConfiguration { get; set; }
    }
}
