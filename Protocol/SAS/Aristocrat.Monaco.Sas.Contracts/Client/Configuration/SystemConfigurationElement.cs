namespace Aristocrat.Monaco.Sas.Contracts.Client.Configuration
{
    using SASProperties;

    /// <summary>
    /// Definition of the SystemConfiguration class.
    /// </summary>
    public class SystemConfigurationElement
    {
        /// <summary>
        /// Gets or sets the list control mappings for the Sas connections.
        /// </summary>
        public ControlPortsElement ControlPorts { get; set; } = new ControlPortsElement();

        /// <summary>
        /// 
        /// </summary>
        public SasValidationType ValidationType { get; set; }
    }
}
