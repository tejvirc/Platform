namespace Aristocrat.Monaco.Hardware.Serial.EdgeLighting.BeagleBone
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Common;
    using Contracts.Communicator;
    using Contracts.EdgeLighting;
    using Contracts.IO;
    using Contracts.SerialPorts;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    public class BeagleBoneProtocol : IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IDictionary<LightShows, string> LightShowMapping = Enum.GetValues(typeof(LightShows))
            .Cast<LightShows>().Select(x => (Show: x, Name: x.GetAttribute<ShowNameAttribute>()?.ShowName))
            .Where(x => !string.IsNullOrEmpty(x.Name)).ToDictionary(x => x.Show, x => x.Name);

        private const string BeagleBoneComPort = "COM8";
        private const int BaudRate = 38400;
        private const int DataBits = 8;
        private const int MaxBufferLength = 256;
        private const int CommunicationTimeoutMs = 1000;
        private readonly IIO _io;
        private readonly ISerialPortController _physicalLayer;

        private bool _disposed;

        private readonly List<byte> _showMessageHeader = new List<byte>
        {
            0x7a, 0x8b, 0x05, 0x74,
            0x01, 0x00,
            0x01, 0x00,
            0x06, 0x00, 0x00, 0x00
        };

        private LightShows CurrentShow { get; set; } = LightShows.Default;

        public BeagleBoneProtocol()
            : this(
                ServiceManager.GetInstance().GetService<IIO>(),
                new SerialPortController())
        {
        }

        public BeagleBoneProtocol(IIO io, ISerialPortController serialPortController)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _physicalLayer = serialPortController ?? throw new ArgumentNullException(nameof(serialPortController));

            _physicalLayer.Configure(
                new ComConfiguration
                {
                    PortName = BeagleBoneComPort,
                    Mode = ComConfiguration.RS232CommunicationMode,
                    BaudRate = BaudRate,
                    DataBits = DataBits,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadBufferSize = MaxBufferLength,
                    WriteBufferSize = MaxBufferLength,
                    ReadTimeoutMs = SerialPort.InfiniteTimeout,
                    WriteTimeoutMs = CommunicationTimeoutMs,
                    KeepAliveTimeoutMs = CommunicationTimeoutMs
                });
        }

        public void Enable(bool enable)
        {
            _physicalLayer.IsEnabled = true;
            SendShow(LightShows.Default, true);
        }

        public void Disable()
        {
            _physicalLayer.IsEnabled = false;
        }

        public void SendShow(LightShows lightShow, bool forceShow = false)
        {
            if (!forceShow && CurrentShow == lightShow)
            {
                return;
            }

            CurrentShow = lightShow;
            SendCurrentShow();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Disable();
            }

            _disposed = true;
        }

        private void SendCurrentShow()
        {
            if (!LightShowMapping.TryGetValue(CurrentShow, out var showName))
            {
                return;
            }

            SendShowByName(showName);
            _io.SetRedScreenFreeSpinBankShow(CurrentShow == LightShows.Reds);
        }

        private void SendShowByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            Logger.Debug($"Playing the light show: {name}");
            var length = BitConverter.GetBytes((ushort)name.Length);
            var showData = Encoding.ASCII.GetBytes(name);
            _physicalLayer.WriteBuffer(_showMessageHeader.Concat(length).Concat(showData).ToArray());
        }
    }
}
