namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    /// <summary>
    ///     Different responses from the Edge light firmware
    /// </summary>
    public enum ResponseType : byte
    {
        LedConfiguration = 0x61,
        AlternateLedStripConfiguration = 0x75,
        AdditionalConfiguration = 0x76,
        AlternateLedStripConfigurationWithLocation = 0x79,
        InvalidResponse = 0x01,
        DiagnosticReport = 0x24
    }
}