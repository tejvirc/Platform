namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    /// <summary>The Touch Device Types.</summary>
    public enum TouchDeviceType
    {
        /// <summary>Integrated Pen.</summary>
        IntegratedPen = 0x00000001,

        /// <summary>External Pen.</summary>
        ExternalPen = 0x00000002,

        /// <summary>Touch Surface.</summary>
        Touch = 0x00000003,

        /// <summary>Touch Pad.</summary>
        TouchPad = 0x00000004

        // TypeMax =  0xFFFFFFFF
    }
}