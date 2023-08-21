namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     This interface defines methods that asp Client must implement.
    ///     asp client would be connected to a unique comm port.
    /// </summary>
    public interface IAspClient
    {
        /// <summary>
        ///     Gets if the link is up
        /// </summary>
        bool IsLinkUp { get; }

        /// <summary>
        ///     Run the client on a thread
        /// </summary>
        bool Start(string commPort);

        /// <summary>
        ///     Stop the running thread
        /// </summary>
        void Stop();
    }
}