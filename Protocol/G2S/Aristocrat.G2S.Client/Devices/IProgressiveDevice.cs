namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <inheritdoc cref="IProgressiveDevice" />
    /// <summary>
    ///     Provides a mechanism to interact with the Progressive device
    /// </summary>
    public interface IProgressiveDevice : IDevice, IRestartStatus, INoResponseTimer
    {
        /// <summary>
        ///     Get Progressive Identifier value
        /// </summary>
        int ProgressiveId { get; }

        /// <summary>
        ///     Get No Progressive Info
        /// </summary>
        int NoProgressiveInfo { get; }

        /// <summary>
        ///     Get whether valid progressive info has been received.
        /// </summary>
        bool ProgressiveInfoValid { get; }

        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     GetProgressiveHostInfo to  query the progressive identifiers and levels that
        ///     the host supports
        /// </summary>
        /// <param name="command">getProgressiveHostInfo command</param>
        /// <param name="timeOut">timeOut value</param>
        /// <returns>progressiveHostInfo</returns>
        progressiveHostInfo GetProgressiveHostInfo(getProgressiveHostInfo command, TimeSpan timeOut);

        /// <summary>
        ///     To send ProgressiveCommit command to host and receive progressiveCommitAck from host
        /// </summary>
        /// <param name="command">ProgressiveHit command</param>
        /// <param name="progressiveStates">progressive state</param>
        /// <param name="progressiveLog">Progressive Log</param>
        /// <returns>ProgressiveCommitAck</returns>
        Task<progressiveCommitAck> ProgressiveCommit(
            progressiveCommit command,
            progressiveLog progressiveLog,
            t_progStates progressiveStates);

        /// <summary>
        ///     To send progressiveHit command to host and receive SetProgressiveWin from host
        /// </summary>
        /// <param name="command">progressiveHit command</param>
        /// <param name="progressiveStates">progressive state</param>
        /// <param name="progressiveLog">progressive Log</param>
        /// <param name="timeout">timeout value</param>
        /// <returns>setProgressiveWin</returns>
        Task<setProgressiveWin> ProgressiveHit(
            progressiveHit command,
            t_progStates progressiveStates,
            progressiveLog progressiveLog,
            TimeSpan timeout);

        /// <summary>
        /// Inform the device that new progressive values were received
        /// and the monitoring timer should be restarted. 
        /// </summary>
        void ResetProgressiveInfoTimer();

        /// <summary>
        /// Update the progressive device with new state information from the host
        /// </summary>
        /// <param name="command">the received message from the host</param>
        void SetProgressiveState(setProgressiveState command);
    }
}