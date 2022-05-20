namespace Aristocrat.Monaco.Sas.Contracts
{
    /// <summary>
    ///     This enumeration presents all possible states involved in a ticket redemption
    /// </summary>
    public enum SasVoucherInState
    {
        /// <summary>
        ///     The idle state, waiting for a ticket.
        /// </summary>
        Idle,

        /// <summary>
        ///     The ticket has been inserted and exception 67 posted.
        /// </summary>
        ValidationRequestPending,

        /// <summary>
        ///     The ticket has been inserted and exception 67 posted but there is an existing LP 71 Ack pending.
        /// </summary>
        ValidationRequestPendingWithAcknowledgementPending,

        /// <summary>
        ///     We got a valid LP 70.
        /// </summary>
        ValidationDataPending,

        /// <summary>
        ///     We got a valid LP 71.
        /// </summary>
        RequestPending,

        /// <summary>
        ///     We have processed LP 71 and are waiting for an Ack.
        /// </summary>
        AcknowledgementPending
    }
}
