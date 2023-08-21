namespace Aristocrat.Monaco.Gaming.Contracts.Process
{
    /// <summary>
    ///     Contract for communicating with a separate process
    /// </summary>
    public interface IProcessCommunication
    {
        /// <summary>
        ///     Starts the IPC
        /// </summary>
        void StartComms();

        /// <summary>
        ///     Stops the IPC
        /// </summary>
        void EndComms();
    }
}
