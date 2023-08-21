namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     The KeepAlive message is sent by the VLT as a heartbeat to maintain connected status with the
    ///     site controller.
    /// </summary>
    public class KeepAlive : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }
    }
}
