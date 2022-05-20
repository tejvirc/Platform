namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <summary>
    ///     Defines the IDownloadDevice interface
    /// </summary>
    public interface IDownloadDevice : IDevice, INoResponseTimer, ISingleDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets a value indicating whether abort transfer supported flag
        /// </summary>
        bool AbortTransferSupported { get; }

        /// <summary>
        ///     Gets a value for authentication retries
        /// </summary>
        int AuthenticationWaitRetries { get; }

        /// <summary>
        ///     Gets a value for the authentication timeout, milliseconds.
        /// </summary>
        int AuthenticationWaitTimeOut { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether downloading is enabled.
        /// </summary>
        bool DownloadEnabled { get; set; }

        /// <summary>
        ///     Gets a value indicating whether uploading is enabled.
        /// </summary>
        bool UploadEnabled { get; }

        /// <summary>
        ///     Gets a value for minimum Package list entries.
        /// </summary>
        int MinPackageListEntries { get; }

        /// <summary>
        ///     Gets a value for minimum Package log entries.
        /// </summary>
        int MinPackageLogEntries { get; }

        /// <summary>
        ///     Gets a value for minimum script list entries.
        /// </summary>
        int MinScriptListEntries { get; }

        /// <summary>
        ///     Gets a value for minimum script log entries.
        /// </summary>
        int MinScriptLogEntries { get; }

        /// <summary>
        ///     Gets a value for no message timer, milliseconds
        /// </summary>
        int NoMessageTimer { get; }

        /// <summary>
        ///     Gets a value indicating whether PauseSupported
        /// </summary>
        bool PauseSupported { get; }

        /// <summary>
        ///     Gets a value indicating whether ProtocolListSupport
        /// </summary>
        bool ProtocolListSupport { get; }

        /// <summary>
        ///     Gets a value indicating whether ScriptingEnabled
        /// </summary>
        bool ScriptingEnabled { get; }

        /// <summary>
        ///     Gets a value for TransferProgressFrequency
        /// </summary>
        int TransferProgressFrequency { get; }

        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     Sends status command to the target host.
        /// </summary>
        /// <param name="command">Status command</param>
        /// <param name="endTime">Send status end time.</param>
        /// <param name="hostAcknowledgedCallBack">Callback to signal host acknowledged status.</param>
        /// <returns>Task</returns>
        Task SendStatus(c_baseCommand command, DateTime endTime, Action hostAcknowledgedCallBack = null);

        /// <summary>
        ///     Sends status command to the target host.
        /// </summary>
        /// <param name="command">Status command</param>
        /// <param name="hostAcknowledgedCallBack">Callback to signal host acknowledged status.</param>
        /// <returns>Task</returns>
        Task SendStatus(c_baseCommand command, Action hostAcknowledgedCallBack = null);
    }
}