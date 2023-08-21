namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT to the site controller to determine what type of card is specified
    ///     (i.e. Employee Card, Player Card, etc.) by any of up to three Card String Tracks.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell <see cref="T:Aristocrat.Mgam.Client.Services.Identification.IdentificationService"/> to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public class GetCardType : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets CardStringTrack1.
        /// </summary>
        public string CardStringTrack1 { get; set; }

        /// <summary>
        ///     Gets or sets CardStringTrack2.
        /// </summary>
        public string CardStringTrack2 { get; set; }

        /// <summary>
        ///     Gets or sets CardStringTrack3.
        /// </summary>
        public string CardStringTrack3 { get; set; }
    }
}
