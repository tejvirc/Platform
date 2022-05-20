namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using Contracts;

    public struct Header
    {
        public int TotalSize => HeaderSize + MaxStripIdSupported * EdgeLightConstants.StrideLength;

        public int Version => 1;

        public int HeaderSize => DefaultStripOffset + EdgeLightConstants.StrideLength;

        public int PlatformOffset => 1 * EdgeLightConstants.StrideLength;

        public int GameOffset => 4 * EdgeLightConstants.StrideLength;

        public int MaxLedInStrip => EdgeLightConstants.MaxLedPerStrip;

        public int MaxStripIdSupported => EdgeLightConstants.MaxStripNum;

        public int BytesPerLed => 4; // A, R, G, B

        public int DefaultStripOffset => 6 * EdgeLightConstants.StrideLength;

        public int[] DataBytes => new[]
        {
            TotalSize, Version, HeaderSize, PlatformOffset, GameOffset, MaxLedInStrip, MaxStripIdSupported,
            BytesPerLed, DefaultStripOffset
        };
    }
}