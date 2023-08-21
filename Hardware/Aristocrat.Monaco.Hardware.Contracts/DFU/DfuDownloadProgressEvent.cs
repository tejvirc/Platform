namespace Aristocrat.Monaco.Hardware.Contracts.Dfu
{
    using System.Globalization;
    using Kernel;

    /// <summary>Definition of the DfuDownloadProgressEvent class.</summary>
    /// <remarks>This event is posted by the DFU service to indicate download progress.</remarks>
    public class DfuDownloadProgressEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DfuDownloadProgressEvent" /> class.
        /// </summary>
        /// <param name="progress">
        ///     The progress percentage (-1 to 100).  -1 indicates ready to start download, 0 indicates starting
        ///     download.
        /// </param>
        public DfuDownloadProgressEvent(int progress)
        {
            Progress = progress;
        }

        /// <summary>Gets the progress percentage for the DFU download.</summary>
        public int Progress { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Progress={1}]",
                GetType().Name,
                Progress);
        }
    }
}