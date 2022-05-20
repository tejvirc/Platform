namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT to request the termination of an open session.  If the Session balances are zero, then SRC_DoNotPrintVoucher will be returned. 
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell TODO: to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    ///     Implementing <see cref="ISessionId"/> 
    /// </remarks>
    public class EndSession : Request, IInstanceId, ISessionId, ILocalTransactionId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        /// The identifier of the session to end 
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// A local identifier for this transaction. 
        /// </summary>
        public int LocalTransactionId { get; set; }
    }
}
