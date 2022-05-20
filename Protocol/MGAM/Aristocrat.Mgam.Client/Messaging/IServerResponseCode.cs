namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Wraps responses to site controller.
    /// </summary>
    public interface IServerResponseCode
    {
        /// <summary>
        ///     Gets the response status code.
        /// </summary>
        ServerResponseCode ResponseCode { get; }
    }
}
