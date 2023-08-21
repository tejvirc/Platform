namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    /// <summary>
    ///     The validation results received from the host
    /// </summary>
    public class HostValidationResults
    {
        /// <summary>
        ///     Creates a HostValidationResults instance
        /// </summary>
        /// <param name="systemId">The system ID received from the host</param>
        /// <param name="validationNumber">The validation number received from the host</param>
        public HostValidationResults(byte systemId, string validationNumber)
        {
            SystemId = systemId;
            ValidationNumber = validationNumber;
        }

        /// <summary>
        ///     Gets the system ID received from the host
        /// </summary>
        public byte SystemId { get; }

        /// <summary>
        ///     Gets the validation Number received from the host
        /// </summary>
        public string ValidationNumber { get; }
    }
}