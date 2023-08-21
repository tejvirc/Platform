namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Notify VLT of Progressive Winner, provide
    ///     display text.
    /// </summary>
    public class ProgressiveWinner : Command
    {
        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        public string Message { get; set; }
    }
}
