namespace Aristocrat.Monaco.Sas.Contracts.SASProperties
{
    using System.ComponentModel;

    /// <summary>
    ///     Enum containing the validation types available if validation is supported.
    /// </summary>
    public enum SasValidationType
    {
        /// <summary>
        ///     Standard validation or no validation support
        /// </summary>
        [Description("None")]
        None,

        /// <summary>
        ///     Secure Enhanced Validation
        /// </summary>
        [Description("Secure Enhanced")]
        SecureEnhanced,

        /// <summary>
        ///     System Validation
        /// </summary>
        [Description("System")]
        System
    }

    /// <summary>
    ///     Enum containing meter models required to be reported by SAS.
    /// </summary>
    public enum SasMeterModel
    {
        /// <summary>
        ///     Meter model is not specified
        /// </summary>
        NotSpecified,
        /// <summary>
        ///     Credits are metered when won
        /// </summary>
        MeteredWhenWon,
        /// <summary>
        ///     Credits are metered when played or paid
        /// </summary>
        MeteredWhenPlayed
    }
}
