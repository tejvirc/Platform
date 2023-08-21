namespace Aristocrat.Mgam.Client.Options
{
    using System;
    using System.Net;

    /// <summary>
    ///     Manages protocol options.
    /// </summary>
    public class ProtocolOptions : Options<ProtocolOptions>
    {
        private IPEndPoint _directoryAddress;
        private IPAddress _networkAddress;
        private IPEndPoint _directoryResponseAddress;
        private bool _serverCertificateValidation;

        /// <summary>
        ///     Initializes an instance of the <see cref="ProtocolOptions"/> class.
        /// </summary>
        public ProtocolOptions()
        {
            CurrentValue = new ProtocolOptions(this);
        }

        /// <summary>
        ///     Initializes an instance of the <see cref="ProtocolOptions"/> class.
        /// </summary>
        /// <param name="builder"><see cref="ProtocolOptionsBuilder"/>.</param>
        public ProtocolOptions(ProtocolOptionsBuilder builder)
        {
            var protocolExtension = builder.Options.FindExtension<ProtocolOptionsExtension>();
            if (protocolExtension == null)
            {
                throw new ArgumentException($@"{nameof(ProtocolOptionsExtension)} extension found", nameof(builder));
            }

            _directoryAddress = protocolExtension.DirectoryAddress;
            _networkAddress = protocolExtension.NetworkAddress;
            _directoryResponseAddress = protocolExtension.DirectoryResponseAddress;
            _serverCertificateValidation = protocolExtension.ServerCertificateValidation;

            CurrentValue = new ProtocolOptions(this);
        }

        /// <summary>
        ///     Initializes an instance of the <see cref="ProtocolOptions"/> class.
        /// </summary>
        /// <param name="options">Source options.</param>
        private ProtocolOptions(ProtocolOptions options)
        {
            _directoryAddress = options._directoryAddress;
            _networkAddress = options._networkAddress;
            _directoryResponseAddress = options._directoryResponseAddress;
            _serverCertificateValidation = options._serverCertificateValidation;
        }

        /// <summary>
        ///     Gets or sets the Directory service address.
        /// </summary>
        public virtual IPEndPoint DirectoryAddress
        {
            get => _directoryAddress;

            set => SetOption(ref _directoryAddress, value);
        }

        /// <summary>
        ///     Gets or sets the Directory service response address.
        /// </summary>
        public virtual IPEndPoint DirectoryResponseAddress
        {
            get => _directoryResponseAddress;

            set => SetOption(ref _directoryResponseAddress, value);
        }

        /// <summary>
        ///     Gets or sets the local network address.
        /// </summary>
        public virtual IPAddress NetworkAddress
        {
            get => _networkAddress;

            set => SetOption(ref _networkAddress, value);
        }

        /// <summary>
        ///     Gets or sets the local network address.
        /// </summary>
        public virtual bool ServerCertificateValidation
        {
            get => _serverCertificateValidation;

            set => SetOption(ref _serverCertificateValidation, value);
        }
    }
}
