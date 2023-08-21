namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent in response to a <see cref="GetCardType"/> message.
    /// </summary>
    public class GetCardTypeResponse : Response
    {
        /// <summary>
        ///     Gets or sets CardString.
        /// </summary>
        public string CardString { get; set; }

        /// <summary>
        ///     Gets or sets CardString.
        /// </summary>
        public string CardType { get; set; }
    }
}
