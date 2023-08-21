namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using Gds;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    ///     Definition of the IGdsCommunicator interface.
    /// </summary>
    public interface IGdsCommunicator : ICommunicator
    {
        /// <summary>
        ///     Event when a data message is received from GDS device.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EventHandler<GdsSerializableMessage> MessageReceived;

        /// <summary>
        ///     Get the firmware revision.
        /// </summary>
        string FirmwareRevision { get; }

        /// <summary>
        ///     Get the boot firmware version.
        /// </summary>
        string BootVersion { get; }

        /// <summary>
        ///     Get the variant name.
        /// </summary>
        string VariantName { get; }

        /// <summary>
        ///     Get the variant version.
        /// </summary>
        string VariantVersion { get; }

        /// <summary>
        ///     Send a data message to GDS device.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="token">The cancellation token</param>
        void SendMessage(GdsSerializableMessage message, CancellationToken token);
    }
}