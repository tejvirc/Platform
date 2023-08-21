namespace Aristocrat.Bingo.Client.Messages
{
    /// <summary>
    ///     Wraps responses from the bingo client
    /// </summary>
    public interface IResponse : IMessage
    {
        ResponseCode ResponseCode { get; }
    }
}