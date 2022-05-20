namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Defines the InstanceId property.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IInstanceId"/> interface will tell the message sender to set
    ///     the InstanceID property to the active instance.
    ///     InstanceId based on the registered VLT Service being targeted.
    /// </remarks>
    public interface IInstanceId
    {
        /// <summary>
        ///     Gets the instance ID.
        /// </summary>
        int InstanceId { get; set; }
    }
}
