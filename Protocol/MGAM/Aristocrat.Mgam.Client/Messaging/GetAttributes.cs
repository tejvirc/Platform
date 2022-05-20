namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message will cause the site controller to send all attributes (along with their current values),
    ///     to the requesting client.
    /// </summary>
    public class GetAttributes : Request, IInstanceId
    {
        /// <inheritdoc />
        public int InstanceId { get; set; }
    }
}
