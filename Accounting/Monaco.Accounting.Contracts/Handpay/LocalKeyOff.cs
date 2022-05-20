namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    /// <summary>
    ///     When the handpay device is owned by a G2S host, the localKeyOff attribute controls which types of key-off
    ///     are permitted locally at the EGM prior to receiving a setRemoteKeyOff command from the host
    /// </summary>
    public enum LocalKeyOff
    {
        /// <summary>
        ///     Any local key-off method permitted in the handpayRequest command MAY
        ///     be performed locally prior to host authorization.
        /// </summary>
        AnyKeyOff,

        /// <summary>
        ///     Only local key-offs as a handpay MAY be performed locally prior to host
        ///     authorization.
        /// </summary>
        HandpayOnly,

        /// <summary>
        ///     No local key-offs MAY be performed locally prior to host authorization.
        /// </summary>
        NoKeyOff
    }
}
