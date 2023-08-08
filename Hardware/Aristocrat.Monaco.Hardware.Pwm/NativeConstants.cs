namespace Aristocrat.Monaco.Hardware.Pwm
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal class NativeConstants
    {
        /// <summary> Constants for errors </summary>
        internal const uint ErrorFileNotFound = 2;
        internal const uint ErrorInvalidName = 123;
        internal const uint ErrorAccessDenied = 5;
        internal const uint ErrorIoPending = 997;

        /// <summary> Constants for return value </summary>
        internal const int InvalidHandleValue = -1;

        /// <summary> Constants for dwFlagsAndAttributes </summary>
        internal const uint FileFlagOverlapped = 0x40000000;
        internal const uint FileFlagNoBuffering = 0x20000000;

        /// <summary> Constants for dwCreationDisposition </summary>
        internal const uint OpenExisting = 3;

        /// <summary> Constants for dwDesiredAccess </summary>
        internal const uint GenericRead = 0x80000000;
        internal const uint GenericWrite = 0x40000000;
        internal const uint FileShareRead = 0x00000001;
        internal const uint FileShareWrite = 0x00000002;

        /// <summary> Constants for WaitForSingleObject's return value </summary>
        internal const uint StatusWait0 = 0x0;
        internal const uint WaitObject0 = StatusWait0 + 0;
        internal const uint WaitTimeout = 258;

        /// <summary> Constants for IOCT call to identify devices </summary>
        internal const uint CoinAcceptorDeviceType = 0x8100;
        internal const uint NVCoinAcceptorDeviceType = 0x8200;
        internal const uint HopperDeviceType = 0x8300;



        internal const uint MethodBuffered = 0;
        internal const uint FileAnyAccess = 0;

        /// <summary> Configuration Manager CONFIGRET return status codes </summary>
        internal const int CrSuccess = 0x0;

        /// <summary>Flags for CM_Get_Device_Interface_List, CM_Get_Device_Interface_List_Size </summary>
        internal const uint CmGetDeviceInterfaceListPresent = 0x00000000; // only currently 'live' device interfaces
        internal const uint CmGetDeviceInterfaceListAllDevices = 0x00000001; // all registered device interfaces, live or not
        internal uint CmGetDeviceInterfaceListBits = 0x00000001;

        /// <summary>Function that creates IOCTL code for the TPCI940.</summary>
        /// <param name="command">Function Creates IOCTL code for the TPCI940</param>
        /// <returns>I/O control code (IOCTL)</returns>
        public static uint COINACCEPTOR_MAKE_IOCTL(uint DeviceType, CoinAcceptorCommands command)
        {
            return CTL_CODE(DeviceType, 0x800 + (uint)command, MethodBuffered, FileAnyAccess);
        }

        public static uint PWMDEVICE_MAKE_IOCTL<C>(uint DeviceType, C command)
        {
            var value = (uint)((IConvertible)command).ToInt32(CultureInfo.InvariantCulture.NumberFormat);
            return CTL_CODE(DeviceType, 0x800 + value, MethodBuffered, FileAnyAccess);
        }


        /// <summary>Function used to create a unique system I/O control code (IOCTL).</summary>
        /// <param name="deviceType">Defines the type of device for the given IOCTL</param>
        /// <param name="function">Defines an action within the device category</param>
        /// <param name="method">Defines the method codes for how buffers are passed for I/O and file system controls</param>
        /// <param name="access">Defines the access check value for any access</param>
        /// <returns>I/O control code (IOCTL)</returns>
        private static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
        {
            return (deviceType << 16) | (access << 14) | (function << 2) | method;
        }
    }
    internal enum CoinAcceptorCommands
    {
        CoinAcceptorRecordCount = 1,
        CoinAcceptorRejectOn,
        CoinAcceptorRejectOff,
        CoinAcceptorDivertorOn,
        CoinAcceptorDivertorOff,
        CoinAcceptorRegisterValue,
        CoinAcceptorPollingCount,
        CoinAcceptorPeek,
        CoinAcceptorAcknowledge,
        CoinAcceptorSetInputRegister,
        CoinAcceptorStartPolling,
        CoinAcceptorStopPolling
    }
    internal enum Cc62Signals
    {
        None,
        SenseSignal = (1 << 2),
        CreditSignal = (1 << 3),
        AlarmSignal = (1 << 4),
        SolenoidSignal = (1 << 5)
    }

    internal enum SenseSignalState
    {
        HighToLow,
        LowToHigh,
        Fault
    }

    internal enum CreditSignalState
    {
        HighToLow,
        LowToHigh,
        Fault
    }

    internal enum AlarmSignalState
    {
        HighToLow,
        LowToHigh,
        Fault
    }
    internal enum DivertorAction
    {
        None,
        DivertToHopper,
        DivertToCashbox,
    }
    internal enum CoinEntryStatus : byte
    {
        Read,
        InProcess,
        Ack
    }
    internal class CoinSignalsConsts
    {
        /* Coin entry input masks */

        /* Sense pulse lengths
            0..4    = noise
            5..40   = valid
            41...   = sesnse fault
        */
        internal const uint SensePulseMin = 5;       /* Min Sense pulse length */
        internal const uint SensePulseMax = 40;      /* Max Sense pulse length */

        /* Credit pulse lengths
            0..4    = noise
            5..40   = valid
            41...   = credit fault
        */
        internal const uint CreditPulseMin = 5;       /* Min Credit pulse length */
        internal const uint CreditPulseMax = 40;      /* Max Credit pulse length */

        /* Alarm pulse lengths
            0..5    = noise
            6..240  = yoyo
            241...  = coin in optic fault
        */
        internal const uint AlarmPulseMin = 5;       /* Min Credit pulse length */
        internal const uint AlarmPulseMax = 240;     /* Max Credit pulse length */

        internal const uint SenseToCreditMaxtime = 250;     /* Max allowed time from sense to credit pulse */

        internal const uint CoinTransitTime = 1000;
    }

    internal enum HopperCommands
    {
        HopperSetType = 1,
        HopperEnable,
        HopperPollingCount,
        HopperChangeCount,
        HopperSetOutputs,
        HopperClrOutputs,
        HopperTestDisconnection,
        HopperGetRegisterValue
    }

    internal class HopperMasks
    {
        internal const int HopperLowMask = 0x10;   /* Hopper level low mask (not used)*/
        internal const int HopperHighMask = 0x20;   /* Hopper level high mask */
        internal const int HopperCoinOutMask = 0x40;    /* Coin out sensor */
        internal const int HopperOvercurrentMask = 0x80;    /* Overcurrent sensor (not used)*/
        internal const int HopperMotorDirMask = 0x01;    /* Motor direction mask (not used)*/
        internal const int HopperMotorDriveMask = 0x02;    /* Motor drive mask */
        internal const int HopperSensorMask = 0x04;    /* Sensor drive mask */
        internal const int HopperDisconnectMask = 0x8000; /* Hopper disconnect mask */
    }
    internal class HopperConsts
    {
        internal const uint HopperDisconnetTime = 1000; //ms
        internal const uint HopperDisconnectThreshold = 3;
        internal const uint HopperDisconnectTime = 1000; /* counts between hopper disconnect    checks */
        internal const uint HopperMaxBlockedTime = 400; /* coin jam if > HCD_MAX_BLOCKED_TIME */
        internal const uint HopperDebouncedTime = 3; /* discard pulses < HCD_DEBOUNCE_TIME */
        internal const uint HopperEmptyTime = 6000; /* scan for HCD_HOPPER_EMPTY_TIME to see coin */
        internal const uint HopperPauseTime = 2000; /* time to pause if didn't see coin */
        internal const uint HopperMaxTos = 1; /* max no. of pauses */
        internal const uint HopperMotorOffTimer = 70; /* if idle for longer than 60 ms reset motor vars */
    }

    internal enum HopperTaskState
    {
        WaitingForLeadingEdge,   /* Waiting for front edge of coin */
        WaitingForTrailingEdge,  /* Waiting for back edge of coin */
        WaitingForTimeout,        /* Pausing */
        WaitingForReset           /* Waiting to be reset */
    }
}
