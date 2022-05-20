namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    /// <summary>
    ///     Different type of commands that the firmware supports.
    ///     Right now these are written as per amalgamation of
    ///     all the f/w in the field. Need to check which all
    ///     needs to be there for the generic f/w.
    /// </summary>
    public enum RequestType : byte
    {
        SelfTest = 0x04,
        SelfTestResult = 0x05,
        SoftwareReset = 0x06,
        RequestBoardStatus = 0x08,
        RequestCrc = 0x09,
        DummyCommand = 0x0B,
        GetLedConfiguration = 0x60,
        Channel1LedFrame = 0x62,
        Channel2LedFrame = 0x63,
        Channel3LedFrame = 0x64,
        Channel4LedFrame = 0x65,
        SetLowPowerMode = 0x66,
        SetDeviceLedBrightness = 0x67, // Set's for the whole device
        SetAttractMode = 0x71,
        RunSequence = 0x72,
        UpdateVirtualStrip = 0x73,
        SetStripSpecialAction = 0x74,
        SetBarkeeperLedBrightness = 0x77,
        SetLegacyData = 0x6B,
        LegacyReplyData = 0x6C,
        SetVerveData = 0x6D,
        InvalidCommand = 0x01
    }
}