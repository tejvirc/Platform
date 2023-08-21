namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines methods that SAS Clients must implement.
    /// There may be multiple SAS clients on an EGM.
    /// Each SAS client would be connected to a unique comm port.
    /// </summary>
    public interface ISasClient
    {
        /// <summary>
        ///     Gets the client number for this SAS client
        /// </summary>
        byte ClientNumber { get; }

        /// <summary>
        ///     Gets if the link is up
        /// </summary>
        bool LinkUp { get; }

        /// <summary>
        ///      Indicates whether real time event reporting is active or not
        /// </summary>
        bool IsRealTimeEventReportingActive { set; get; }

        /// <summary>
        ///     Sends a response that doesn't need crc attached to the comm port associated with this client
        /// </summary>
        /// <param name="command">The command to send</param>
        void SendResponse(IReadOnlyCollection<byte> command);

        /// <summary>
        ///     Attempts to open the given comm port for use by this client
        /// </summary>
        /// <param name="portName">The comm port to use. For example "COM5"</param>
        /// <returns>true if the comm port is opened without errors, false otherwise</returns>
        bool AttachToCommPort(string portName);

        /// <summary>
        ///     Closes the comm port attached to this client
        /// </summary>
        void ReleaseCommPort();

        /// <summary>
        ///     Initialize the client
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Run the client on a thread
        /// </summary>
        void Run();

        /// <summary>
        ///     Stop the running thread
        /// </summary>
        void Stop();

        /// <summary>
        ///     Return SASclient configuration
        /// </summary>
        SasClientConfiguration Configuration { get; }
    }
}
