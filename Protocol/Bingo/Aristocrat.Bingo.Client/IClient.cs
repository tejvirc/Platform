namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Threading.Tasks;
    using Configuration;

    /// <summary>
    ///     The client interface
    /// </summary>
    public interface IClient : IDisposable
    {
        /// <summary>
        ///     This event is fired when the client is connected.
        /// </summary>
        event EventHandler<ConnectedEventArgs> Connected;

        /// <summary>
        ///     This event is fired when the client is disconnected.
        /// </summary>
        event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     This event is fired when a message is received.
        /// </summary>
        event EventHandler<EventArgs> MessageReceived;

        /// <summary>
        ///     This is event is fired when the connection state changes
        /// </summary>
        event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        /// <summary>
        ///     The client's configuration.
        /// </summary>
        ClientConfigurationOptions Configuration { get; }

        /// <summary>
        ///     Starts the client running
        /// </summary>
        /// <returns>The task for starting the client</returns>
        Task<bool> Start();

        /// <summary>
        ///     Stops the client from running
        /// </summary>
        /// <returns>The task for stopping the client</returns>
        Task<bool> Stop();

        /// <summary>
        ///     The client's unique firewall rule name.
        /// </summary>
        string FirewallRuleName { get; }
    }
}