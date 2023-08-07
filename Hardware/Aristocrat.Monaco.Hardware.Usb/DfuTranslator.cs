namespace Aristocrat.Monaco.Hardware.Usb
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Contracts.Communicator;
    using log4net;

    /// <summary>A dfu command wrapper implementation.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Communicator.IDfuTranslator" />
    public class DfuTranslator : IDfuTranslator
    {
        private const int DfuVersion = 272;
        private const int DfuVersionOld = 257;
        private const int HostToDeviceDataPhaseDir = 0x21;
        private const int DeviceToHostDataPhaseDir = 0xA1;
        private const int BufferSize = 256;
        private const int CreationAttempts = 10;

        private const byte DetachCommand = 0;
        private const byte DownloadCommand = 1;
        private const byte UploadCommand = 2;
        private const byte GetStatusCommand = 3;
        private const byte ClearStatusCommand = 4;
        private const byte GetStateCommand = 5;
        private const byte AbortCommand = 6;
        private const byte UsbResetCommand = 7;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private UsbK _device;
        private int _interfaceNumber = -1;

        /// <summary>Gets or sets the identifier of the vendor.</summary>
        /// <value>The identifier of the vendor.</value>
        public int VendorId { get; set; }

        /// <summary>Gets or sets the identifier of the product.</summary>
        /// <value>The identifier of the product.</value>
        public int ProductId { get; set; }

        /// <inheritdoc />
        public bool Detach(int timeout)
        {
            try
            {
                var timeoutDuration = timeout;
                var buffer = new byte[0];
                return DfuCommand(
                    HostToDeviceDataPhaseDir,
                    DetachCommand,
                    (ushort)timeoutDuration,
                    GetIndexFromInterface(_interfaceNumber),
                    0,
                    buffer,
                    out _);
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: detach command error - {e.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public int Download(int blockNumber, byte[] buffer, int count)
        {
            try
            {
                WinUsbSetupPacket packet;
                packet.RequestType = HostToDeviceDataPhaseDir;
                packet.Request = DownloadCommand;
                packet.Value = (ushort)blockNumber;
                packet.Index = GetIndexFromInterface(_interfaceNumber);
                packet.Length = 0;

                ClaimInterface(_interfaceNumber);
                var result = _device.ControlTransfer(
                    packet,
                    buffer,
                    (uint)count,
                    out var bytesTransferred,
                    IntPtr.Zero);
                ReleaseInterface(_interfaceNumber);
                if (!result)
                {
                    return -1;
                }

                return (int)bytesTransferred;
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: download command error - {e.Message}");
                return -1;
            }
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
            try
            {
                var buffer = new byte[6];
                if (!DfuCommand(
                    DeviceToHostDataPhaseDir,
                    GetStatusCommand,
                    0,
                    GetIndexFromInterface(_interfaceNumber),
                    0,
                    buffer,
                    out _))
                {
                    return null;
                }

                return new DfuDeviceStatus
                {
                    Status = (DfuStatus)buffer[0],
                    Timeout = 0x10000 * buffer[3] + 0x100 * buffer[2] + buffer[1],
                    State = (DfuState)buffer[4]
                };
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: get status command error - {e.Message}");
                return null;
            }
        }

        /// <inheritdoc />
        public bool ClearStatus()
        {
            try
            {
                var buffer = new byte[0];
                return DfuCommand(
                    HostToDeviceDataPhaseDir,
                    ClearStatusCommand,
                    0,
                    GetIndexFromInterface(_interfaceNumber),
                    0,
                    buffer,
                    out _);
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: clear status command error - {e.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public bool Abort()
        {
            try
            {
                var buffer = new byte[0];
                return DfuCommand(
                    HostToDeviceDataPhaseDir,
                    AbortCommand,
                    0,
                    GetIndexFromInterface(_interfaceNumber),
                    0,
                    buffer,
                    out _);
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: abort command error - {e.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public DfuState GetState()
        {
            try
            {
                var buffer = new byte[1];
                if (!DfuCommand(
                    DeviceToHostDataPhaseDir,
                    GetStateCommand,
                    0,
                    GetIndexFromInterface(_interfaceNumber),
                    0,
                    buffer,
                    out _))
                {
                    return DfuState.Unknown;
                }

                return (DfuState)buffer[0];
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: get state command error - {e.Message}");
                return DfuState.Unknown;
            }
        }

        /// <inheritdoc />
        public DfuCapabilities Capabilities()
        {
            if (_device == null)
            {
                return null;
            }

            GCHandle? pinnedPacket = null;
            try
            {
                var buffer = new byte[BufferSize];
                // get detailed descriptors
                if (!ControlCommand(
                    (byte)BmRequestDir.DeviceToHost,
                    0,
                    0,
                    (byte)UsbRequestEnum.UsbRequestGetDescriptor,
                    (byte)UsbDescriptorType.Configuration,
                    0,
                    0,
                    buffer,
                    out var transferredLength))
                {
                    return null;
                }

                pinnedPacket = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var memAddress = pinnedPacket.Value.AddrOfPinnedObject();
                var bytesToProcess = transferredLength;

                while (bytesToProcess >= Marshal.SizeOf(typeof(UsbCommonDescriptor)))
                {
                    var commonDescriptorBox = (UsbCommonDescriptor?)Marshal.PtrToStructure(
                        memAddress,
                        typeof(UsbCommonDescriptor));
                    if (commonDescriptorBox == null)
                        throw new ArgumentNullException(nameof(memAddress));
                    var commonDescriptor = commonDescriptorBox.Value;

                    if (bytesToProcess < commonDescriptor.bLength)
                    {
                        break;
                    }

                    switch ((UsbDescriptorType)
                        commonDescriptor.bDescriptorType)
                    {
                        case UsbDescriptorType.DfuFunctional:
                        {
                            var descBox =
                                (UsbDfuFunctionalDescriptor?)Marshal.PtrToStructure(
                                    memAddress,
                                    typeof(UsbDfuFunctionalDescriptor));
                            if (descBox == null)
                                throw new ArgumentNullException(nameof(memAddress));
                            var desc = descBox.Value;
                            if (desc.bcdDFUVersion == DfuVersionOld || desc.bcdDFUVersion == DfuVersion)
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

                        break;
                    }

                    bytesToProcess -= commonDescriptor.bLength;
                    memAddress += commonDescriptor.bLength;
                }

                return null;
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: error in getting capabilities - {e.Message}");
                return null;
            }
            finally
            {
                pinnedPacket?.Free();
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
            if (disposing)
            {
                _device?.Free();
                _device = null;
            }
        }

        /// <summary>Send DFU command to the device.</summary>
        /// <param name="requestType">This represents the type of usb request.</param>
        /// <param name="request">This represents the command type like DfuDetach, DfuGetState, etc.</param>
        /// <param name="value">Value parameter of usb request.</param>
        /// <param name="index">Index parameter of usb request.</param>
        /// <param name="length">Length parameter of usb request.</param>
        /// <param name="buffer">Data parameter of usb request.</param>
        /// <param name="bytesTransferred">[out] Number of bytes transferred in data phase of the command.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        private bool DfuCommand(
            byte requestType,
            byte request,
            ushort value,
            ushort index,
            ushort length,
            byte[] buffer,
            out uint bytesTransferred)
        {
            WinUsbSetupPacket packet;
            packet.RequestType = requestType;
            packet.Request = request;
            packet.Value = value;
            packet.Index = index;
            packet.Length = length;
            bytesTransferred = 0;

            ClaimInterface(_interfaceNumber);

            Thread.Sleep(10);

            var retVal = _device.ControlTransfer(
                packet,
                buffer,
                (uint)buffer.Length,
                out bytesTransferred,
                IntPtr.Zero);
            Thread.Sleep(10);
            ReleaseInterface(_interfaceNumber);
            if (!retVal)
            {
                GetErrorText(); // log this error
            }

            return retVal;
        }

        /// <summary>Control command.</summary>
        /// <param name="direction">The direction.</param>
        /// <param name="type">The type.</param>
        /// <param name="receipient">The receipient.</param>
        /// <param name="requestCode">This represents the command type like DfuDetach, DfuGetState, etc.</param>
        /// <param name="valueHigh">The value high.</param>
        /// <param name="valueLow">The value low.</param>
        /// <param name="index">Zero-based index of the.</param>
        /// <param name="data">Reference to the data that is sent to the device or to be received from the device.</param>
        /// <param name="transferCount">[out] Number of bytes transferred in data phase of the command.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        private bool ControlCommand(
            byte direction,
            byte type,
            byte receipient,
            byte requestCode,
            byte valueHigh,
            byte valueLow,
            ushort index,
            byte[] data,
            out uint transferCount)
        {
            var retVal = false;
            WinUsbSetupPacket packet;
            packet.RequestType = (byte)((direction << 7) | (type << 5) | receipient);
            packet.Request = requestCode;
            packet.Value = (ushort)((valueHigh << 8) | valueLow);
            packet.Index = index;
            transferCount = 0;

            var buffer = data ?? new byte[BufferSize];
            var bufferLength = (uint)buffer.Length;
            packet.Length = (ushort)bufferLength;

            ClaimInterface(_interfaceNumber);
            if (_device.ControlTransfer(packet, buffer, bufferLength, out transferCount, IntPtr.Zero))
            {
                retVal = true;
            }
            ReleaseInterface(_interfaceNumber);
            return retVal;
        }

        private void ClaimInterface(int @interface)
        {
            if (@interface == -1)
            {
                return;
            }

            _device.ClaimInterface((byte)@interface, false);
        }

        private void ReleaseInterface(int @interface)
        {
            if (@interface == -1)
            {
                return;
            }

            _device.ReleaseInterface((byte)@interface, false);
        }

        private bool Device(int vendorId, int productId)
        {
            LstK list = null;
            try
            {
                list = new LstK(KlstFlag.None);
                list.MoveReset();
                while (list.MoveNext(out var info))
                {
                    Logger.Info(
                        $"Device - VendorId: {info.Common.Vid}, ProductId:{info.Common.Pid}, " +
                        $"Connected:{info.Connected}, Interface: {info.Common.MI}");
                    if (info.Common.Vid == vendorId &&
                        info.Common.Pid == productId &&
                        info.Connected)
                    {
                        _device = new UsbK(info);
                        _interfaceNumber = info.Common.MI == -1 ? 0 : info.Common.MI;
                        return true;
                    }
                }

                return false;
            }
            catch (NullReferenceException e)
            {
                Logger.Error($"DfuTranslator: error in getting device - {e.Message}");
                return false;
            }
            finally
            {
                list?.Free();
            }
        }

        private ushort GetIndexFromInterface(int interfaceNumber)
        {
            return (ushort)(interfaceNumber & 0xff);
        }

        private static string GetErrorText()
        {
            var errorCode = Marshal.GetLastWin32Error();
            return new Win32Exception(errorCode).Message;
        }
    }
}