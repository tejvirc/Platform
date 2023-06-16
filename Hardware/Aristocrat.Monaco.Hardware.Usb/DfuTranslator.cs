namespace Aristocrat.Monaco.Hardware.Usb
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Contracts.Communicator;
    using log4net;
    using NativeUsb.Usb;

    /// <summary>A dfu command wrapper implementation.</summary>
    /// <seealso cref="IDfuTranslator" />
    public class DfuTranslator : IDfuTranslator
    {
        private const int DfuVersion = 272;
        private const int DfuVersionOld = 257;
        private const int BufferSize = 256;
        private const int CreationAttempts = 10;

        private const byte DetachCommand = 0;
        private const byte DownloadCommand = 1;
        private const byte GetStatusCommand = 3;
        private const byte ClearStatusCommand = 4;
        private const byte AbortCommand = 6;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private INativeUsbDevice _device;
        private int _interfaceNumber = -1;
        private bool _disposed;

        /// <summary>Gets or sets the identifier of the vendor.</summary>
        /// <value>The identifier of the vendor.</value>
        public int VendorId { get; set; }

        /// <summary>Gets or sets the identifier of the product.</summary>
        /// <value>The identifier of the product.</value>
        public int ProductId { get; set; }

        /// <inheritdoc />
        public bool Detach(int timeout)
        {
            if (_device is null)
            {
                return false;
            }

            var header = new ControlHeader(
                UsbRequestTypes.Interface | UsbRequestTypes.Class,
                DetachCommand,
                (ushort)timeout,
                GetIndexFromInterface(_interfaceNumber),
                0);
            var result = _device.ControlTransferToDevice(header);
            return result.Match(_ => true, _ => false);
        }

        /// <inheritdoc />
        public int Download(int blockNumber, byte[] buffer, int count)
        {
            if (_device is null)
            {
                return -1;
            }

            var header = new ControlHeader(
                UsbRequestTypes.Interface | UsbRequestTypes.Class,
                DownloadCommand,
                (ushort)blockNumber,
                GetIndexFromInterface(_interfaceNumber),
                0);
            var result = _device.ControlTransferToDevice(header);
            return result.Match(l => l, _ => -1);
        }

        /// <inheritdoc />
        public int Upload(int blockNumber, byte[] buffer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool CommsReset()
        {
            try
            {
                return _device != null && _device.ResetDevice();
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: reset command error - {e.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public DfuDeviceStatus GetStatus()
        {
            const int statusLength = 6;
            if (_device is null)
            {
                return null;
            }

            var header = new ControlHeader(
                UsbRequestTypes.Class | UsbRequestTypes.Interface,
                GetStatusCommand,
                0,
                GetIndexFromInterface(_interfaceNumber),
                statusLength);
            var result = _device.ControlTransferFromDevice(header);
            return result.Match(
                buffer => new DfuDeviceStatus
                {
                    Status = (DfuStatus)buffer[0],
                    Timeout = 0x10000 * buffer[3] + 0x100 * buffer[2] + buffer[1],
                    State = (DfuState)buffer[4]
                },
                _ => null!);
        }

        /// <inheritdoc />
        public bool ClearStatus()
        {
            if (_device is null)
            {
                return false;
            }

            var header = new ControlHeader(
                UsbRequestTypes.Class | UsbRequestTypes.Interface,
                ClearStatusCommand,
                0,
                GetIndexFromInterface(_interfaceNumber),
                0);
            var result = _device.ControlTransferToDevice(header);
            return result.Match(_ => true, _ => false);
        }

        /// <inheritdoc />
        public bool Abort()
        {
            if (_device is null)
            {
                return false;
            }

            var header = new ControlHeader(
                UsbRequestTypes.Class | UsbRequestTypes.Interface,
                AbortCommand,
                0,
                GetIndexFromInterface(_interfaceNumber),
                0);
            var result = _device.ControlTransferToDevice(header);
            return result.Match(_ => true, _ => false);
        }

        /// <inheritdoc />
        public DfuState GetState()
        {
            if (_device is null)
            {
                return DfuState.Unknown;
            }

            var header = new ControlHeader(
                UsbRequestTypes.Class | UsbRequestTypes.Interface,
                AbortCommand,
                0,
                GetIndexFromInterface(_interfaceNumber),
                0);
            var result = _device.ControlTransferFromDevice(header);
            return result.Match(buffer => (DfuState)buffer[0], _ => DfuState.Unknown);
        }

        /// <inheritdoc />
        public DfuCapabilities Capabilities()
        {
            if (_device == null)
            {
                return null;
            }

            try
            {
                var buffer = new byte[BufferSize];
                var result = _device.ControlTransferFromDevice(
                    new ControlHeader(
                        UsbRequestTypes.Standard | UsbRequestTypes.Device,
                        (byte)UsbRequest.GetDescriptor,
                        (byte)UsbDescriptorType.Configuration << 8,
                        0,
                        BufferSize),
                    buffer);
                return result.Match(
                    length => ParseCapabilities(buffer, length),
                    s =>
                    {
                        Logger.Error($"Failed to get capabilities.  Got Error: {s}");
                        return null!;
                    });
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: error in getting capabilities - {e.Message}");
                return null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Initializes this object.</summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public bool Initialize()
        {
            var attempts = CreationAttempts;
            while (attempts-- > 0)
            {
                if (Device(VendorId, ProductId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Dispose resources
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _device?.Dispose();
                _device = null;
            }

            _disposed = true;
        }

        private static DfuCapabilities ParseCapabilities(byte[] buffer, int transferredLength)
        {
            GCHandle? pinnedPacket = null;
            try
            {
                pinnedPacket = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var memAddress = pinnedPacket.Value.AddrOfPinnedObject();
                var bytesToProcess = transferredLength;

                while (bytesToProcess >= Marshal.SizeOf(typeof(UsbCommonDescriptor)))
                {
                    var commonDescriptor = (UsbCommonDescriptor)Marshal.PtrToStructure(
                        memAddress,
                        typeof(UsbCommonDescriptor));

                    if (bytesToProcess < commonDescriptor.bLength)
                    {
                        break;
                    }

                    if ((UsbDescriptorType)commonDescriptor.bDescriptorType == UsbDescriptorType.DfuFunctional)
                    {
                        var desc = (UsbDfuFunctionalDescriptor)Marshal.PtrToStructure(
                            memAddress,
                            typeof(UsbDfuFunctionalDescriptor));

                        if (desc.bcdDFUVersion is DfuVersionOld or DfuVersion)
                        {
                            return new DfuCapabilities
                            {
                                CanDownload = (desc.bmAttributes & 0x01) != 0,
                                CanUpload = (desc.bmAttributes & 0x02) != 0,
                                ManifestationTolerant = (desc.bmAttributes & 0x04) != 0,
                                WillDetach = (desc.bmAttributes & 0x08) != 0,
                                DetachTimeOut = desc.wDetachTimeOut,
                                TransferSize = desc.wTransferSize
                            };
                        }
                    }

                    bytesToProcess -= commonDescriptor.bLength;
                    memAddress += commonDescriptor.bLength;
                }

                return null;
            }
            finally
            {
                pinnedPacket?.Free();
            }
        }

        private bool Device(int vendorId, int productId)
        {
            _device = NativeUsbDeviceFactory.CreateNativeUsbDevice(new UsbDeviceDetails(vendorId, productId));
            return true;
        }

        private static ushort GetIndexFromInterface(int interfaceNumber)
        {
            return (ushort)(interfaceNumber & 0xff);
        }
    }
}