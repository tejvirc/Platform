namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent from the VLT to the site controller to purchase a New York Lottery Video Lottery Ticket.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell TODO: to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public class RequestPlay : Request, IInstanceId, ISessionId, ILocalTransactionId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     The identifier of the previous session to resume
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        ///     Amount of cash, in cents, in the current session 
        /// </summary>
        public int SessionCashBalance { get; set; }

        /// <summary>
        ///     Current coupon balance in the current session in cents 
        /// </summary>
        public int SessionCouponBalance { get; set; }

        /// <summary>
        ///     Index of the paytable to use for this game session 
        /// </summary>
        public int PayTableIndex { get; set; }

        /// <summary>
        ///     Number of credits for this play session 
        /// </summary>
        public int NumberOfCredits { get; set; }

        /// <summary>
        ///     Cost of a single ticket in cents 
        /// </summary>
        public int Denomination { get; set; }

        /// <summary>
        ///     Identifies the theme of this game 
        /// </summary>
        public int GameUpcNumber { get; set; }

        /// <summary>
        ///     A local identifier for this transaction
        /// </summary>
        public int LocalTransactionId { get; set; }
    }
}
