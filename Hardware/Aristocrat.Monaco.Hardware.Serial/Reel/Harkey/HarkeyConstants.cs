namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Contracts.Reel;
    using Protocols;

    public static class HarkeyConstants
    {
        public const byte DataLengthMask = 0xF0;
        public const byte MaxSequenceId = 0x7F; // Undocumented max value.  Found via testing
        public const int MaxReelId = 6;
        public const int DefaultRampTable = 0;
        public const int DefaultNudgeDelay = 0;
        public const int Acknowledged20 = 0x20;
        public const int PollingIntervalMs = 1000;
        public const int SpinningPollingIntervalMs = 3000;
        public const int ExpectedResponseTime = 50;
        public const int LightsPerReel = 3;
        public const int MaxLightId = MaxReelId * LightsPerReel;
        public const int NumberOfMotorSteps = 200;
        public const int NumberOfStops = 22;
        public const int AllowableResponseTime = ExpectedResponseTime * 2;

        public static readonly IMessageTemplate MessageTemplate = new MessageTemplate<XorChecksumEngine>(
            new List<MessageTemplateElement>
            {
                new()
                {
                    ElementType = MessageTemplateElementType.ConstantDataLengthMask,
                    Value = new[] { DataLengthMask },
                    Length = 1,
                    IncludedInCrc = true
                },
                new()
                {
                    ElementType = MessageTemplateElementType.VariableData, IncludedInCrc = true
                },
                new()
                {
                    ElementType = MessageTemplateElementType.Crc, IncludedInCrc = false, Length = 1
                }
            },
            0);

        public static readonly IReadOnlyList<ReelLampData> AllLightsOn = Enumerable.Range(0, MaxLightId)
            .Select(i => new ReelLampData(Color.White, true, i + 1)).ToArray();

        public static readonly IReadOnlyList<ReelLampData> AllLightsOff = Enumerable.Range(0, MaxLightId)
            .Select(i => new ReelLampData(Color.White, false, i + 1)).ToArray();
    }

    [Flags]
    public enum GlobalStatus : byte
    {
        None = 0x00,
        ReelVoltageLow = 0x01,
        LampVoltageLow = 0x02
    }

    [Flags]
    public enum ReelStatus : byte
    {
        None = 0x00,
        RmConnected = 0x01,
        RmsConnected = 0x02,
        Stalled = 0x10,
        ReelOutOfSync = 0x20,
        ReelSlowSpin = 0x40,
        ReelInError = 0x80
    }

    public enum HomeReelResponseCodes : byte
    {
        AcceptedReelMask = 0x10,
        AcceptedReel1 = 0x11,
        AcceptedReel2 = 0x12,
        AcceptedReel3 = 0x13,
        AcceptedReel4 = 0x14,
        AcceptedReel5 = 0x15,
        AcceptedReel6 = 0x16,
        Homed = 0x20,
        FailedHome = 0x90
    }

    public enum SpinResponseCodes : byte
    {
        AllReelsStopped = 0x20,
        StoppingReelMask = 0x20,
        StoppingReel1 = 0x21,
        StoppingReel2 = 0x22,
        StoppingReel3 = 0x23,
        StoppingReel4 = 0x24,
        StoppingReel5 = 0x25,
        StoppingReel6 = 0x26,
        Accepted = 0x80
    }

    public enum HarkeyCommandId : byte
    {
        HomeReel = 0x40,
        CommandResponse = 0xA4,
        GetRm6Version = 0x4F,
        GetLightControllerVersion = 0x55,
        GetStatus = 0x48,
        SetFaults = 0x45,
        ChangeSpeed = 0x44,
        SpinToGoal = 0x43,
        SpinFreely = 0x42,
        Nudge = 0x41,
        Reset = 0x56,
        SetReelLightColor = 0x57,
        SetReelBrightness = 0x58,
        AbortAndSlowSpin = 0xF0,
        SetReelLights = 0xF1,
        SoftReset = 0xF2,
        UnsolicitedErrorTamperResponse = 0xB2,
        UnsolicitedErrorStallResponse = 0xB3,
        ProtocolErrorResponse = 0xC2,
        HardwareErrorResponse = 0xD2
    }

    public enum HarkeyRequestErrorCodes : byte
    {
        ErrorMask = 0x70,
        InvalidValue = 0x70,
        ReelInError = 0x71,
        ReelAlreadySpinning = 0x72,
        BadState = 0x73,
        OutOfSync = 0x74,
        ReelNotAvailable = 0x75,
        LowVoltage = 0x76,
        GameChecksumError = 0x7E,
        FaultChecksumError = 0x7F
    }

    public enum HarkeyResponseErrorCodes : byte
    {
        NotAvailable = 0x90,
        NoSync = 0x91,
        Skew = 0x92,
        Stall = 0x93,
    }

    public enum MessageMaskType : byte
    {
        Command = 0xF0,
        CommandResponse = 0xA0,
        UnsolicitedErrorResponse = 0xB0,
        ProtocolErrorResponse = 0xC0,
        HardwareErrorResponse = 0xD0
    }

    public enum HardwareErrorCodes
    {
        GameSerialPortError = 0x01,
        PortReset = 0x02,
        WdtReset = 0x03,
        BrownOutReset = 0x04,
        PowerUpReset = 0x05,
        ResetFromStackOverflow = 0x06,
        ResetFromStackUnderflow = 0x07,
        ResetInstructionFromFirmware = 0x08,
        MotorVoltageLow = 0x09,
        LampVoltageLow = 0x0A,
        Com1Reset = 0x0E,
        UnknownReset = 0x0F,
        ProgrammingNeeded = 0xEE
    }
}