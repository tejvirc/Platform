namespace Aristocrat.Monaco.Sas.Contracts.SASProperties
{
    using System.ComponentModel;

    /// <summary>
    /// The fund transfer type which can be Aft or Eft.
    /// </summary>
    public enum FundTransferType
    {
        /// <summary>
        /// Advanced Funds Transfer
        /// </summary>
        [Description("AFT")]
        Aft,

        /// <summary>
        /// Electronic Funds Transfer
        /// </summary>
        [Description("EFT")]
        Eft
    }
}