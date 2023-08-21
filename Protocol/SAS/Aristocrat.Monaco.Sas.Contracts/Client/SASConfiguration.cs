namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using Configuration;

    /// <summary>
    /// Definition of the SasConfiguration class.
    /// </summary>
    public class SasConfiguration
    {
        /// <summary>
        /// Gets or sets the system configuration.
        /// </summary>
        public SystemConfigurationElement System { get; set; } = new SystemConfigurationElement();

        /// <summary>
        /// Gets or sets the hand pay configuration.
        /// </summary>
        public HandPayConfigurationElement HandPay { get; set; } = new HandPayConfigurationElement();
    }
}
