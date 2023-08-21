namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent to the site controller after the VLT determines that all configuration and
    ///     initialization is finished, and the VLT is ready for play.
    /// </summary>
    public class ReadyToPlay : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }
    }
}
