namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Wraps a message or command response.
    /// </summary>
    public interface IResponse : IMessage, IServerResponseCode
    {
    }
}
