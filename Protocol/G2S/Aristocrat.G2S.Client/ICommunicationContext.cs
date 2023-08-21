namespace Aristocrat.G2S.Client
{
    /// <summary>
    ///     Defines a startup context for a communication device
    /// </summary>
    public interface ICommunicationContext
    {
        /// <summary>
        ///     Gets a value indicating whether indicates whether the device structure of the EGM has been reset since the last
        ///     commsOnLine command.Always set to
        ///     true when a new communications device is added to the EGM.
        /// </summary>
        bool DeviceReset { get; }

        /// <summary>
        ///     Gets a value indicating whether indicates whether the active device structure  has changed or the owner,
        ///     configurator, or guest permissions of the
        ///     host have changed in  relation to one or more active devices since the last commsOnLine command.Always set to true
        ///     when a new communications device is added to the EGM.
        /// </summary>
        bool DeviceChanged { get; }

        /// <summary>
        ///     Gets a value indicating whether indicates whether meter or event subscriptions of the host have been lost.
        /// </summary>
        bool SubscriptionLost { get; }

        /// <summary>
        ///     Gets a value indicating whether indicates whether the meters for one or more devices have been reset.
        /// </summary>
        bool MetersReset { get; }

        /// <summary>
        ///     Gets a value indicating whether indicates whether the active device structure has changed since the last
        ///     commsOnLine
        ///     command.Always set to true
        ///     when a new communications device is added to the EGM.
        /// </summary>
        bool DeviceStateChanged { get; }

        /// <summary>
        ///     Gets a value indicating whether indicates whether the owner, configurator or guest permissions of the host have
        ///     been
        ///     changed since the last
        ///     commsOnLine command. Always set to true when a new communications device is added to the EGM.
        /// </summary>
        bool DeviceAccessChanged { get; }
    }
}