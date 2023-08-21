namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     The <i>storage</i> class is used to determine whether the EGM has sufficient storage space to store and install
    ///     selected packages.
    /// </summary>
    /// <remarks>
    ///     The class includes the command used to query the state of the coin acceptor. It also includes commands used to
    ///     enable and disable the coin acceptor and to query the types of coins accepted by the coin acceptor.
    /// </remarks>
    public class StorageDevice : ClientDeviceBase<storage>, IStorageDevice
    {
        private const string GtkPrefix = @"GTK_";

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorDevice" /> class.
        /// </summary>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public StorageDevice(IDeviceObserver deviceObserver)
            : base(1, deviceObserver)
        {
        }

        /// <inheritdoc />
        public override string DevicePrefix => GtkPrefix;

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
        }
    }
}