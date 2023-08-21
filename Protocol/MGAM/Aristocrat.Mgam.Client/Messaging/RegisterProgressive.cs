namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is optional and is only for games that will contribute to a progressive pool. This
    ///     message will notify the site controller and cause progressive attributes to be updated.The VLT
    ///     must have registered applicable progressive attributes.
    /// </summary>
    public class RegisterProgressive : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets the name of the progressive jackpot to link to.
        /// </summary>
        public string ProgressiveName { get; set; }

        /// <summary>
        ///     Gets or sets the cost in pennies of current session game play.
        /// </summary>
        public int TicketCost { get; set; }

        /// <summary>
        ///     Gets or sets the name of an instance scope, int attribute registered
        ///     previously by the VLT that will be set periodically to the
        ///     current value of the jackpot.
        /// </summary>
        public string SignValueAttributeName { get; set; }

        /// <summary>
        ///     Gets or sets the name of an instance scope, string attribute
        ///     registered earlier by the VLT that may be set to contain a
        ///     promotional text message for display on the progressive
        ///     sign.
        /// </summary>
        public string SignMessageAttributeName { get; set; }
    }
}
