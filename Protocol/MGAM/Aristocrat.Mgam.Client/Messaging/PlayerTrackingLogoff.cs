namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///    This message is sent to disassociate a player tracking card with VLT activity. This message
    ///     should be sent by the VLT when a player tracking card is removed.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface with tell <see cref="T:Aristocrat.Mgam.Client.Services.Identification.IdentificationService"/> to add the
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public class PlayerTrackingLogoff : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }
    }
}
