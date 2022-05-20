namespace Aristocrat.Bingo.Client
{
    using Grpc.Core;

    public interface IClientEndpointProvider<out T> where T : ClientBase
    {
        /// <summary>
        ///     Gets the current client callback channel
        /// </summary>
        T Client { get; }

        /// <summary>
        ///     Gets whether or not the client is connected
        /// </summary>
        bool IsConnected { get; }
    }
}