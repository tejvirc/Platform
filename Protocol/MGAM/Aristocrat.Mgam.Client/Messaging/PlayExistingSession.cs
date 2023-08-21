namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Allows VLT to continue an existing session, for
    ///     example after loss of connection to one VLT
    ///     Service and reconnection to another one.
    /// </summary>
    public class PlayExistingSession : Command
    {
        /// <summary>
        ///     Gets or sets the session ID.
        /// </summary>
        public int SessionId { get; set; }
    }
}
