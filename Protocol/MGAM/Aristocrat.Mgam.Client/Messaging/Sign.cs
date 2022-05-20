namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Update Message Display on VLT, for example
    ///     for a progressive display.
    /// </summary>
    public class Sign : Command
    {
        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        public string Message { get; set; }
    }
}
