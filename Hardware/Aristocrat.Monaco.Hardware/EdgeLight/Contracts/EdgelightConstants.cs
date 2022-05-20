namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    /// <summary>
    ///     List down the limitations in the Edge light firmware/device
    /// </summary>
    public static class EdgeLightConstants
    {
        /// <summary>
        ///     Max physical channels from firmware
        /// </summary>
        public const int MaxChannelsPerBoard = 4;

        /// <summary>
        ///     Max Virtual Channels Per Board as supported from firmware
        /// </summary>
        public const int MaxVirtualChannelsPerBoard = 250;

        /// <summary>
        ///     MaxLedPerStrip
        /// </summary>
        public const int MaxLedPerStrip = 200;

        /// <summary>
        ///     ArgbBytesPerLed
        /// </summary>
        public const int ArgbBytesPerLed = 4;

        /// <summary>
        ///     DefaultReportTimeout
        /// </summary>
        public const int DefaultReportTimeout = 250;

        /// <summary>
        ///     MaxStripNum as supported from firmware
        /// </summary>
        public const int MaxStripNum = MaxChannelsPerBoard + MaxVirtualChannelsPerBoard;

        /// <summary>
        ///     MaxChannelBrightness as supported from firmware
        /// </summary>
        public const int MaxChannelBrightness = 100;

        /// <summary>
        ///     NoOfTimesDummyCommandToSend to the new firmware
        /// </summary>
        public const int NoOfTimesDummyCommandToSend = 2;

        /// <summary>
        ///     StrideLength
        /// </summary>
        public const int StrideLength = ArgbBytesPerLed * MaxLedPerStrip;

        /// <summary>
        ///     Rgb Bytes Per Led as needed by firmware
        /// </summary>
        public const int RgbBytesPerLed = 3;

        /// <summary>
        ///     VendorId for Aristocrat Edge Light
        /// </summary>
        public const int VendorId = 0x126c;

        /// <summary>
        ///     ProductId for Aristocrat Edge Light
        /// </summary>
        public const int ProductId = 0x4352;
    }
}