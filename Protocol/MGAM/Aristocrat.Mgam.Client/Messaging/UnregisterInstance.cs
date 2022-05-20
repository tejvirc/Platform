namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent by the VLT to the site controller to unregister the current instance. 
    /// </summary>
    public class UnregisterInstance : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }
    }
}
