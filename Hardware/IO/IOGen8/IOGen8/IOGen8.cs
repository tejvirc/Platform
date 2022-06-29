namespace Vgt.Client12.Hardware.IO
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Cabinet;
    using Aristocrat.Monaco.Hardware.Contracts.IO;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using Aristocrat.Monaco.Hardware.Contracts.TowerLight;
    using Aristocrat.Monaco.Kernel;
    using log4net;
    using Microsoft.Win32.SafeHandles;

    /// <summary>Implements the IO Implementation interface for the Gen8 device.</summary>
    public sealed class IOGen8 : IIOImplementation, IDisposable
    {
        private const int Tpci940NumInputs = 63;
        private const int Tpci940NumOutputs = 16;
        private const int LogicDoorPhysicalId = 45;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static ulong _outputs;
        private static SafeFileHandle _deviceHandle;

        private readonly Dictionary<int, byte> _doorMasks = new Dictionary<int, byte>();
        private readonly List<InputEvent> _intrusionEvents = new List<InputEvent>();

        private readonly IEventBus _bus;
        private readonly ICabinetDetectionService _cabinetService;

        private byte[] _biosData;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IOGen8" /> class.
        /// </summary>
        public IOGen8()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        public IOGen8(IEventBus bus, ICabinetDetectionService cabinetService)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _cabinetService = cabinetService ?? throw new ArgumentNullException(nameof(cabinetService));

            DeviceConfiguration = new Device { PollingFrequency = DeviceControl.Tpci940Gen8PollingFrequency };

            _doorMasks.Add(LogicDoorPhysicalId, 0x80); // Logic Door
            _doorMasks.Add(46, 0x02); // Top Box Door MS1
            _doorMasks.Add(48, 0x08); // Drop Door MS3
            _doorMasks.Add(49, 0x10); // Main Door MS4
            _doorMasks.Add(50, 0x20); // Cash Door MS5
            _doorMasks.Add(51, 0x40); // Belly Door MS6
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (DeviceConfiguration != null)
            {
                DeviceConfiguration.Dispose();
                DeviceConfiguration = null;
            }

            _disposed = true;
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public Device DeviceConfiguration { get; private set; }

        /// <inheritdoc />
        public int GetMaxInputs => Tpci940NumInputs;

        /// <inheritdoc />
        public int GetMaxOutputs => Tpci940NumOutputs;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong GetInputs
        {
            get
            {
                ulong inputs = 0;

                if (_deviceHandle != null)
                {
                    // Allocate memory for ulong
                    var inputRegister = Marshal.AllocHGlobal(Marshal.SizeOf(inputs));

                    try
                    {
                        // Perform Device I/O Control operation.
                        var success = NativeMethods.DeviceIoControl(
                            _deviceHandle,
                            DeviceControl.TP940DIO_MAKE_IOCTL(Gen8IOCommands.InputData),
                            IntPtr.Zero,
                            0,
                            inputRegister,
                            Marshal.SizeOf(inputs),
                            out _,
                            IntPtr.Zero);

                        var lastWin32Error = Marshal.GetLastWin32Error();

                        if (success == false)
                        {
                            Logger.Error("GetInputs: DeviceIoControl call failed: Read");

                            if (lastWin32Error != 0)
                            {
                                Logger.Error("GetInputs: LastWin32Error: " + lastWin32Error);
                            }

                            _bus.Publish(new ErrorEvent(ErrorEventId.InputFailure));
                            return inputs;
                        }

                        unsafe
                        {
                            inputs = *(ulong*)inputRegister.ToPointer();
                        }

                        Marshal.FreeHGlobal(inputRegister);

                        return inputs & DeviceControl.Tpci940AvailableInputsMask;
                    }
                    catch
                    {
                        Marshal.FreeHGlobal(inputRegister);

                        Logger.Error("GetInputs: DeviceIoControl unhandled exception");

                        _bus.Publish(new ErrorEvent(ErrorEventId.InputFailure));
                        return inputs;
                    }
                }

                Logger.Error("GetInputs: Device Handle not initialized");
                _bus.Publish(new ErrorEvent(ErrorEventId.InputFailure));

                return inputs;
            }
        }

        /// <inheritdoc />
        public InputEvent[] GetIntrusionEvents
        {
            get
            {
                lock (_intrusionEvents)
                {
                    return _intrusionEvents.ToArray();
                }
            }
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong GetOutputs => _outputs;

        /// <inheritdoc />
        [CLSCompliant(false)]
        public int GetWatchdogEnabled => 0;

        public bool WasCarrierBoardRemoved { get; private set; }

        /// <inheritdoc />
        public void Cleanup()
        {
            if (_deviceHandle == null)
            {
                return;
            }

            if (_deviceHandle.IsInvalid)
            {
                return;
            }

            if (GetWatchdogEnabled > 0)
            {
                if (EnableWatchdog(0) != 0)
                {
                    Logger.Error("Could not disable watchdog");
                    _bus.Publish(new ErrorEvent(ErrorEventId.WatchdogDisableFailure));
                }
            }

            _deviceHandle.Close();
            _deviceHandle.SetHandleAsInvalid();
        }

        /// <inheritdoc />
        public void Initialize()
        {
            DeviceConfiguration.Manufacturer = "Aristocrat";
            DeviceConfiguration.Model = "MK7+";
            DeviceConfiguration.VariantName = _cabinetService.Type.ToString();
            DeviceConfiguration.Protocol = HardwareFamilyIdentifier.Identify().ToString();

            try
            {
                _deviceHandle = NativeMethods.CreateFile(
                    DeviceControl.Tpci940FileNameBase,
                    DeviceControl.Tpci940DesiredAccess,
                    DeviceControl.Tpci940ShareMode,
                    DeviceControl.Tpci940SecurityAttributes,
                    DeviceControl.Tpci940FileCreationDistribution,
                    DeviceControl.Tpci940FileAttributes,
                    DeviceControl.Tpci940TemplateFile);
            }
            catch
            {
                Logger.Error("IOGen8(): CreateFile unhandled exception");
                return;
            }

            if (_deviceHandle.IsInvalid)
            {
                Logger.Error($"IOGen8(): Cannot create device handle {DeviceControl.Tpci940FileNameBase}");

                _bus.Publish(new InitCompleteEvent());
                return;
            }

            // Start with watchdog disabled.
            if (EnableWatchdog(0) != 0)
            {
                Logger.Error("Could not disable watchdog");
                _bus.Publish(new ErrorEvent(ErrorEventId.WatchdogDisableFailure));
            }

            // Initialize current outputs as all off
            _outputs = 0;

            var tempVar = Marshal.AllocHGlobal(DeviceControl.Tpci940CardIdLength);

            bool success;

            try
            {
                success = NativeMethods.DeviceIoControl(
                    _deviceHandle,
                    DeviceControl.TP940DIO_MAKE_IOCTL(Gen8IOCommands.InitializeSpiBus),
                    IntPtr.Zero,
                    0,
                    tempVar,
                    DeviceControl.Tpci940CardIdLength,
                    out _,
                    IntPtr.Zero);
            }
            catch
            {
                Marshal.FreeHGlobal(tempVar);

                Logger.Error("IOGen8(): DeviceIoControl: BoardInfo unhandled exception");

                // DeviceIoControl failed to read Board Info. We need to report back to the IOService
                // this error, so that is can report initialization failed.
                _bus.Publish(new ErrorEvent(ErrorEventId.ReadBoardInfoFailure));
                return;
            }

            if (!success)
            {
                Marshal.FreeHGlobal(tempVar);

                Logger.Error("IOGen8(): DeviceIoControl: Call failed: Read BoardInfo");

                _bus.Publish(new ErrorEvent(ErrorEventId.ReadBoardInfoFailure));
                return;
            }

            var tempArray = new byte[DeviceControl.Tpci940CardIdLength];
            for (var i = 0; i < DeviceControl.Tpci940CardIdLength; i++)
            {
                tempArray[i] = 0;
            }

            Marshal.Copy(tempVar, tempArray, 0, DeviceControl.Tpci940CardIdLength);

            Marshal.FreeHGlobal(tempVar);

            for (var i = 0; i < DeviceControl.Tpci940CardIdLength && tempArray[i] != 0; i++)
            {
                DeviceConfiguration.FirmwareId += (char)tempArray[i];
            }

            Thread.Sleep(100);

            foreach (var i in _doorMasks.Keys)
            {
                if (GetPhysicalDoorWasOpened(i))
                {
                    Logger.Debug($"Power off door open event detected for door {i}");
                    lock (_intrusionEvents)
                    {
                        _intrusionEvents.Add(new InputEvent(i, true));
                    }
                }
            }

            WasCarrierBoardRemoved = GetDoorSealValue() == 0xFE;
            SetLogicDoorSealValue(0);
            ResetDoorSeal();

            _bus.Publish(new InitCompleteEvent());
        }

        /// <inheritdoc />
        public int EnableWatchdog(int seconds)
        {
            return 0;
        }

        /// <inheritdoc />
        public int ResetWatchdog()
        {
            return 0;
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public void TurnOutputsOff(ulong outputs)
        {
            // Make sure available outputs are requested
            if ((outputs | DeviceControl.Tpci940AvailableOutputMask) != DeviceControl.Tpci940AvailableOutputMask)
            {
                return;
            }

            Logger.Debug("Outputs turned off");
            _outputs &= ~outputs;
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public void TurnOutputsOn(ulong outputs)
        {
            // Make sure available outputs are requested
            if ((outputs | DeviceControl.Tpci940AvailableOutputMask) != DeviceControl.Tpci940AvailableOutputMask)
            {
                return;
            }

            Logger.Debug("Outputs turned on");
            _outputs |= outputs;
        }

        /// <inheritdoc />
        public void SetMechanicalMeterLight(bool lightEnabled)
        {
            DeviceControl.Ioctl(_deviceHandle, lightEnabled ? 1 : 0, Gen8IOCommands.MechanicalLight);
        }

        /// <inheritdoc />
        public int SetMechanicalMeter(int meterMask)
        {
            var result = 0;

            if (!DeviceControl.Ioctl(_deviceHandle, meterMask, ref result, Gen8IOCommands.SetMechanicalMeter))
            {
                Logger.Debug("failed setting mechanical meter");
            }

            return result;
        }

        /// <inheritdoc />
        public int ClearMechanicalMeter(int meterMask)
        {
            var result = 0;

            if (!DeviceControl.Ioctl(_deviceHandle, meterMask, ref result, Gen8IOCommands.ClearMechanicalMeter))
            {
                Logger.Debug("failed clearing mechanical meter");
            }

            return result;
        }

        /// <inheritdoc />
        public int StatusMechanicalMeter(int meterMask)
        {
            uint result = 0;

            if (!DeviceControl.IoctlUint(
                _deviceHandle,
                (uint)meterMask,
                ref result,
                Gen8IOCommands.StatusMechanicalMeter))
            {
                Logger.Debug("failed getting mechanical meter status");
            }

            return (int)(result & meterMask);
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public uint SetButtonLamp(uint lightIndex, bool lightStatusOn)
        {
            var lightMask = lightIndex | (lightStatusOn ? 0xff00u : 0x0000u);
            uint result = 0;

            if (!_deviceHandle.IsInvalid && !DeviceControl.IoctlUint(
                _deviceHandle,
                lightMask,
                ref result,
                Gen8IOCommands.ButtonLamp))
            {
                Logger.Debug("failed resetting Button Lamp");
            }

            return result;
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public uint SetButtonLampByMask(uint lightBits, bool lightStatusOn)
        {
            var lightMask = lightBits | (lightStatusOn ? 0xff0000u : 0x0000u);
            uint result = 0;

            if (!_deviceHandle.IsInvalid && !DeviceControl.IoctlUint(
                _deviceHandle,
                lightMask,
                ref result,
                Gen8IOCommands.ButtonLampByMask))
            {
                Logger.Debug("failed resetting Button Lamp");
            }

            return result;
        }

        /// <inheritdoc />
        public void ResetDoorSeal()
        {
            if (!DeviceControl.Ioctl(_deviceHandle, Gen8IOCommands.SecResetDoorSeal))
            {
                Logger.Debug("failed resetting Door Seal Value");
            }
        }

        /// <inheritdoc />
        public byte GetDoorSealValue()
        {
            var result = 0;

            if (!DeviceControl.Ioctl(_deviceHandle, 0, ref result, Gen8IOCommands.SecReadDoorSeal))
            {
                Logger.Debug("failed reading Door Seal Value");
            }

            Logger.Debug($"door seal value is {result & 0xff}");

            return (byte)(result & 0xff);
        }

        /// <inheritdoc />
        public void SetLogicDoorSealValue(byte sealValue)
        {
            uint result = 0;

            if (!DeviceControl.IoctlUint(_deviceHandle, sealValue, ref result, Gen8IOCommands.SetLogicDoorSeal))
            {
                Logger.Debug("failed setting Logic Door Seal Value");
            }
        }

        /// <summary>Sets the Door Seal value.</summary>
        /// <returns>get the seal latch value (0x80 = door was open, 0xff = door is open)</returns>
        public byte GetLogicDoorSealValue()
        {
            var result = 0;

            if (!DeviceControl.Ioctl(_deviceHandle, 0, ref result, Gen8IOCommands.GetLogicDoorSeal))
            {
                Logger.Debug("failed reading Logic Door Seal Value");
            }

            Logger.Debug($"Logic door seal value is {result & 0xff}");

            return (byte)(result & 0xff);
        }

        /// <inheritdoc />
        public void ResetPhysicalDoorWasOpened(int physicalId)
        {
            if (_doorMasks.ContainsKey(physicalId))
            {
                if (physicalId == LogicDoorPhysicalId)
                {
                    SetLogicDoorSealValue(0);
                }
                else
                {
                    ResetDoorSeal();
                }
            }
        }

        /// <inheritdoc />
        public bool GetPhysicalDoorWasOpened(int physicalId)
        {
            if (_doorMasks.ContainsKey(physicalId))
            {
                if (physicalId == LogicDoorPhysicalId)
                {
                    return (GetLogicDoorSealValue() & 0x80) == 0x80;
                }

                return (GetDoorSealValue() & _doorMasks[physicalId]) != 0;
            }

            return false;
        }

        /// <inheritdoc />
        public void SetKeyIndicator(int keyMask, bool lightEnabled)
        {
        }

        /// <inheritdoc />
        public byte[] GetFirmwareData(FirmwareData location)
        {
            Gen8IOCommands command;

            switch (location)
            {
                case FirmwareData.Bios:
                    if (_biosData != null)
                    {
                        return _biosData;
                    }

                    command = Gen8IOCommands.BiosRead;
                    break;
                case FirmwareData.Fpga:
                    command = Gen8IOCommands.FpgaRead;
                    break;
                default:
                    return new byte[0];
            }

            byte[] data = null;

            var firmwareSize = GetFirmwareSize(location);

            // Sanity check firmware size. should be in the range of 0-32mb
            if (firmwareSize > 0 && firmwareSize < 1 << 25)
            {
                data = new byte[firmwareSize];

                // This may take some time (FPGA is very slow to read)
                DeviceControl.IoctlByteArray(_deviceHandle, firmwareSize, ref data, command);
            }

            // Cache the bios data
            if (command == Gen8IOCommands.BiosRead)
            {
                _biosData = data;
            }

            return data;
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public ulong GetFirmwareSize(FirmwareData location)
        {
            Gen8IOCommands command;

            switch (location)
            {
                case FirmwareData.Bios:
                    command = Gen8IOCommands.BiosSize;
                    break;
                case FirmwareData.Fpga:
                    command = Gen8IOCommands.FpgaSize;
                    break;
                default:
                    return 0;
            }

            ulong firmwareSize = 0;

            // Get Size of firmware
            DeviceControl.Ioctl(_deviceHandle, 0, ref firmwareSize, command);

            // Sanity check firmware size. should be in the range of 0-32mb
            if (firmwareSize > 0 && firmwareSize < 1 << 25)
            {
                return firmwareSize;
            }

            return 0;
        }

        public string GetFirmwareVersion(FirmwareData location)
        {
            var version = string.Empty;

            switch (location)
            {
                case FirmwareData.Bios:
                    // The actual version offset is at 0x00000364, but includes 'Module BIOS'
                    const int versionOffset = 0x0000036F;
                    const int versionLength = 0x0C;

                    var bios = GetFirmwareData(FirmwareData.Bios);
                    if (bios == null)
                    {
                        return null;
                    }

                    try
                    {
                        version = Encoding.UTF8.GetString(bios, versionOffset, versionLength).Trim('\0').Trim('.');
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Logger.Warn("Failed to get firmware version", e);
                        return null;
                    }

                    break;
                case FirmwareData.Fpga:
                    const int majorVersionLength = 7;
                    const int majorVersionStartIndex = 16;
                    const int minorVersionLength = 15;
                    const int minorVersionStartIndex = 0;

                    if (_deviceHandle != null)
                    {
                        var result = DeviceControl.ReadReg32(_deviceHandle, Gen8PCI.Legacy.HWO_FPGA_VERSION);
                        if (result != 0)
                        {
                            var bits = new BitArray(new[] { result });

                            // See https://aristocratgaming.sharepoint.com/sites/globalee/GEN9%20FPGA/Gen9%20FPGA%20Design%20Specifications%20A.pdf for more info
                            version =
                                $"{BitArrayToInt(bits, majorVersionStartIndex, majorVersionLength)}.{BitArrayToInt(bits, minorVersionStartIndex, minorVersionLength)}";
                        }
                    }

                    break;
            }

            return version;
        }

        /// <inheritdoc />
        public bool TestBattery(int batteryIndex)
        {
            int batteryId;
            switch (batteryIndex)
            {
                case 0:
                    batteryId = (int)Gen8PCI.BatteryTest.BT_TEST_1;
                    break;
                case 1:
                    batteryId = (int)Gen8PCI.BatteryTest.BT_TEST_2;
                    break;
                default:
                    return false;
            }

            // Reset Battery Test bits
            DeviceControl.WriteReg32(_deviceHandle, Gen8PCI.Legacy.HWO_BTEST, (int)Gen8PCI.BatteryTest.BT_RESET);
            Logger.Debug($"Battery {batteryIndex} test off");

            var hadSuccessfulMeasurement = false;

            // Up to three attempts to pass
            for (var retry = 0; retry < 3; retry++)
            {
                // Let the test circuit settle.
                Thread.Sleep(100);

                // Start Battery Test of indicated Battery ID; the test lasts 54-62ms
                DeviceControl.WriteReg32(_deviceHandle, Gen8PCI.Legacy.HWO_BTEST, batteryId);
                Logger.Debug($"Battery {batteryIndex} test on");

                // Sample five times
                for (var sample = 0; sample < 5; sample++)
                {
                    Thread.Sleep(2); // wait 2ms
                    var res = DeviceControl.ReadReg32(_deviceHandle, Gen8PCI.Legacy.HWO_BTEST);

                    // any good reading will pass the test
                    if ((res & (int)Gen8PCI.BatteryTest.BT_NFAILBATT) == (int)Gen8PCI.BatteryTest.BT_NFAILBATT)
                    {
                        hadSuccessfulMeasurement = true;
                    }

                    Logger.Debug(
                        $"Battery {batteryIndex} success={hadSuccessfulMeasurement}, retry={retry} sample={sample} ::: BTEST={res:X}");
                }

                // Reset Battery Test bits
                DeviceControl.WriteReg32(_deviceHandle, Gen8PCI.Legacy.HWO_BTEST, (int)Gen8PCI.BatteryTest.BT_RESET);
                Logger.Debug($"Battery {batteryIndex} test off");

                if (hadSuccessfulMeasurement)
                {
                    break;
                }
            }

            Logger.Debug($"Battery {batteryIndex} is {(hadSuccessfulMeasurement ? "good" : "bad")}, exit");
            return hadSuccessfulMeasurement;
        }

        /// <inheritdoc />
        public bool SetTowerLight(int lightIndex, bool lightOn)
        {
            if (lightIndex == (int)LightTier.Strobe)
            {
                return SetStrobeLightState(lightOn);
            }

            uint result = 0;
            var lightMask = (uint)lightIndex & 0xfu;
            var lightStateMask = lightOn ? 0xf0u : 0x0u;

            return DeviceControl.IoctlUint(
                _deviceHandle,
                lightMask | lightStateMask,
                ref result,
                Gen8IOCommands.SetTowerLight);
        }

        public bool SetBellState(bool ringBell)
        {
            const int ringState = 1;
            const int notRingingState = 0;
            return DeviceControl.WriteReg32(
                _deviceHandle,
                Gen8PCI.Legacy.HWO_JPBELL,
                ringBell ? ringState : notRingingState);
        }

        public bool SetRedScreenFreeSpinBankShow(bool bankOn)
        {
            const int bankOnState = 1;
            const int bankOffState = 0;
            return DeviceControl.WriteReg32(_deviceHandle, Gen8PCI.Legacy.HWO_MPIO, bankOn ? bankOnState : bankOffState);
        }

        public bool SetStrobeLightState(bool flashStrobe)
        {
            const int flashingState = 1;
            const int notFlashingState = 0;
            return DeviceControl.WriteReg32(
                _deviceHandle,
                Gen8PCI.Legacy.HWO_COINDIV,
                flashStrobe ? flashingState : notFlashingState);
        }

        private static int BitArrayToInt(BitArray bits, int startIndex, int length)
        {
            var result = 0;

            for (var i = 0; i < length; i++)
            {
                if (bits[i + startIndex])
                {
                    result |= 1 << i;
                }
            }

            return result;
        }

        public int GetFanSpeed()
        {
            var speed = DeviceControl.ReadReg32(_deviceHandle, Gen8PCI.Legacy.HWO_FANSPEED) * 100;

            return speed;
        }

        public int GetFanPwm()
        {
            var Pwm = DeviceControl.ReadReg32(_deviceHandle, Gen8PCI.Legacy.HWO_FANPWM);

            return Pwm;
        }

        public void SetPWN(int pwn)
        {

        }
    }
}