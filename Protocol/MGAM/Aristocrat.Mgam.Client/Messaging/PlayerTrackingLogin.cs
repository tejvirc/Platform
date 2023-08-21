namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///    This message is sent to associate all play requests with a particular player tracking card. VLTs
    ///     use this message to communicate to the site controller when a player tracking card is inserted.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell <see cref="T:Aristocrat.Mgam.Client.Services.Identification.IdentificationService"/> to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public class PlayerTrackingLogin : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets PlayerTrackingString.
        /// </summary>
        public string PlayerTrackingString { get; set; }
    }
}
