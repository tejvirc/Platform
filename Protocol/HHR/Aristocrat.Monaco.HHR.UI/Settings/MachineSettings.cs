namespace Aristocrat.Monaco.Hhr.UI.Settings
{
    using MVVM.Model;

    /// <summary>
    ///     Machine settings.
    /// </summary>
    public class MachineSettings : BaseNotify
    {
        private string _centralServerIpAddress;
        private int _centralServerTcpPortNumber;
        private string _centralServerEncryptionKey;
        private int _centralServerUdpPortNumber;

        /// <summary>
        ///     Gets or sets the central server ip address.
        /// </summary>
        public string CentralServerIpAddress
        {
            get => _centralServerIpAddress;

            set => SetProperty(ref _centralServerIpAddress, value);
        }

        /// <summary>
        ///     Gets or sets the central server tcp port number.
        /// </summary>
        public int CentralServerTcpPortNumber
        {
            get => _centralServerTcpPortNumber;

            set => SetProperty(ref _centralServerTcpPortNumber, value);
        }

        /// <summary>
        ///     Gets or sets the central server encryption key.
        /// </summary>
        public string CentralServerEncryptionKey
        {
            get => _centralServerEncryptionKey;

            set => SetProperty(ref _centralServerEncryptionKey, value);
        }

        /// <summary>
        ///     Gets or sets the central server udp port number.
        /// </summary>
        public int CentralServerUdpPortNumber
        {
            get => _centralServerUdpPortNumber;

            set => SetProperty(ref _centralServerUdpPortNumber, value);
        }
    }
}