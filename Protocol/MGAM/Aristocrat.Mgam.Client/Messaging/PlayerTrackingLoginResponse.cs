namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent in response to <see cref="PlayerTrackingLogin"/>.
    /// </summary>
    public class PlayerTrackingLoginResponse : Response
    {
        /// <summary>
        ///     Gets or sets PlayerName.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        ///     Gets or sets PlayerPoints.
        /// </summary>
        public int PlayerPoints { get; set; }

        /// <summary>
        ///     Gets or sets PromotionalInfo.
        /// </summary>
        public string PromotionalInfo { get; set; }
    }
}
