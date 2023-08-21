namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    ///     This enumeration lists all possible sources for an expiration  
    /// </summary>
    public enum ExpirationOrigin
    {
        /// <summery>Value set by the EGM</summery>
        EgmDefault = 0,

        ///<summery>Set by Long Poll 7B (Set Ticket Data)</summery>
        Combined,

        /// <summery>Set by Long Poll 7D (Extended Validation Status)</summery>
        Independent,

        /// <summery>Set by an AFT transfer or ticket redemption</summery>
        /// <remarks>
        /// There are special rules for setting this value. It is reset when there
        /// are no more restricted credits in the EGM.
        /// </remarks>
        Credits,

        /// <summary>All available origins</summary>
        SizeOfExpirationOrigin,
    }
}
