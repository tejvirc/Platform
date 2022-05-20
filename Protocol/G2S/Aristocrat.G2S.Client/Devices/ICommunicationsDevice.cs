namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Defines the communication triggers that can change the communication states within the CommunicationDevice.
    /// </summary>
    public enum CommunicationTrigger
    {
        /// <summary>
        ///     Comms enabled.
        /// </summary>
        Enabled,

        /// <summary>
        ///     Comms established.
        /// </summary>
        Established,

        /// <summary>
        ///     Comms enabled by the host.
        /// </summary>
        HostEnabled,

        /// <summary>
        ///     Comms disabled by the host.
        /// </summary>
        HostDisabled,

        /// <summary>
        ///     Outbound queue is full.
        /// </summary>
        OutboundOverflow,

        /// <summary>
        ///     Outbound queue is no longer full.
        /// </summary>
        OutboundOverflowCleared,

        /// <summary>
        ///     Comms error.
        /// </summary>
        Error,

        /// <summary>
        ///     Configuration change.
        /// </summary>
        ConfigChange,

        /// <summary>
        ///     Comms disabled.
        /// </summary>
        Disabled,

        /// <summary>
        ///     Inbound queue full.
        /// </summary>
        InboundOverflow,

        /// <summary>
        ///     Comms closed.
        /// </summary>
        Close
    }

    /// <summary>
    ///     Provides a mechanism to interact with and control a Communications device.
    /// </summary>
    /// <typeparam name="TTransportState">Version specific transport state.</typeparam>
    /// <typeparam name="TCommsState">Version specific comms state.</typeparam>
    public interface ICommunicationsDevice<out TTransportState, out TCommsState> : IDevice, INoResponseTimer
    {
        /// <summary>
        ///     Gets a value indicating whether gets the state of the outbound queue.
        /// </summary>
        bool OutboundOverflow { get; }

        /// <summary>
        ///     Gets a value indicating whether gets the state of the inbound queue.
        /// </summary>
        bool InboundOverflow { get; }

        /// <summary>
        ///     Gets the current transport state.
        /// </summary>
        TTransportState TransportState { get; }

        /// <summary>
        ///     Gets the current comms state.
        /// </summary>
        TCommsState State { get; }

        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     Gets a value indicating whether the host is authorized to send multicast messages.
        /// </summary>
        bool AllowMulticast { get; }

        /// <summary>
        ///     Gets a value indicating whether gets a value that indicates whether a communications fault message should be
        ///     displayed if the communications channel is unavailable.
        /// </summary>
        bool DisplayFault { get; }

        /// <summary>
        ///     Sets the keep alive interval.
        /// </summary>
        /// <param name="interval">The new keep alive interval.</param>
        void SetKeepAlive(int interval);

        /// <summary>
        ///     Changes the state of the communications device with the specified trigger.
        /// </summary>
        /// <param name="trigger">CommunicationTrigger to act upon.</param>
        void TriggerStateChange(CommunicationTrigger trigger);

        /// <summary>
        ///     Changes the state of the communications device with the specified trigger.
        /// </summary>
        /// <param name="trigger">CommunicationTrigger to act upon.</param>
        /// <param name="message">The text message to be displayed.</param>
        void TriggerStateChange(CommunicationTrigger trigger, string message);

        /// <summary>
        ///     Configures the device
        /// </summary>
        /// <param name="id">Identifier assigned by the host to the set of changes</param>
        /// <param name="useDefaultConfig">
        ///     Indicates whether the default configuration for the device MUST be used when the EGM
        ///     restarts.
        /// </param>
        /// <param name="requiredForPlay">
        ///     Indicates whether the EGM MUST be disabled if either egmEnabled or hostEnabled is set to
        ///     false.
        /// </param>
        /// <param name="timeToLive">Time-to-live value for requests originated by the device</param>
        /// <param name="noResponseTimer">
        ///     The no-response timer used to determine whether an EGM has lost communications with a
        ///     host.
        /// </param>
        /// <param name="displayFault">
        ///     Indicates whether a communications fault message should be displayed if the communications
        ///     channel is unavailable.
        /// </param>
        void Configure(
            long id,
            bool useDefaultConfig,
            bool requiredForPlay,
            int timeToLive,
            int noResponseTimer,
            bool displayFault);

        /// <summary>
        ///     Notifies the host of any configuration changes, if needed.
        /// </summary>
        void NotifyConfigurationChanged();
    }
}