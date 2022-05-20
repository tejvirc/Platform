namespace Aristocrat.G2S.Client
{
    /// <summary>
    ///     Defines a list of arguments used to start a G2S Client.
    /// </summary>
    public class StartupContext : IStartupContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StartupContext" /> class.
        /// </summary>
        public StartupContext()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StartupContext" /> class.
        /// </summary>
        /// <param name="context">The context to be copied</param>
        public StartupContext(IStartupContext context)
        {
            HostId = context.HostId;
            DeviceReset = context.DeviceReset;
            DeviceChanged = context.DeviceChanged;
            SubscriptionLost = context.SubscriptionLost;
            MetersReset = context.MetersReset;
            DeviceStateChanged = context.DeviceStateChanged;
            DeviceAccessChanged = context.DeviceAccessChanged;
        }

        /// <inheritdoc />
        public int HostId { get; set; }

        /// <inheritdoc />
        public bool DeviceReset { get; set; }

        /// <inheritdoc />
        public bool DeviceChanged { get; set; }

        /// <inheritdoc />
        public bool SubscriptionLost { get; set; }

        /// <inheritdoc />
        public bool MetersReset { get; set; }

        /// <inheritdoc />
        public bool DeviceStateChanged { get; set; }

        /// <inheritdoc />
        public bool DeviceAccessChanged { get; set; }
    }
}