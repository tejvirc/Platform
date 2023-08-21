namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Force the VLT into a lock state.
    /// </summary>
    public class Lock : Command
    {
        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        public string Message { get; set; }
    }
}
