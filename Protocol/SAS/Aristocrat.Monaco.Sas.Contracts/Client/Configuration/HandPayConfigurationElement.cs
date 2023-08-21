namespace Aristocrat.Monaco.Sas.Contracts.Client.Configuration
{
    using SASProperties;

    /// <summary>
    ///     Definition of the HandPayConfiguration class.
    /// </summary>
    public class HandPayConfigurationElement
    {
        /// <summary>
        ///     Initializes a new instance of the HandPayConfigurationElement class.
        /// </summary>
        public HandPayConfigurationElement()
        {
            HandpayReportingType = SasHandpayReportingType.SecureHandpayReporting;
        }

        /// <summary>
        ///     Gets or sets a value indicating the type of handpay reporting supported by the gaming machine
        /// </summary>
        public SasHandpayReportingType HandpayReportingType { get; set; }
    }
}
