namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Custom command message.
    /// </summary>
    public class CustomCommand : Command
    {
        /// <summary>
        ///     Gets or sets the command ID.
        /// </summary>
        public int CommandId { get; set; }

        /// <summary>
        ///     Gets or sets the command parameter.
        /// </summary>
        public string CommandParameter { get; set; }
    }
}
