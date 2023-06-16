namespace Aristocrat.Monaco.Hardware.Usb
{
    using System.Runtime.InteropServices;

    /// <summary>DFU functional descriptor</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UsbDfuFunctionalDescriptor
    {
        /// <Summary>Size of this descriptor (in bytes)</Summary>
        public byte bLength;

        /// <Summary>Descriptor type.</Summary>
        public byte bDescriptorType;

        /// <Summary>Attributes.</Summary>
        public byte bmAttributes;

        /// <summary>Detach timeout.</summary>
        public ushort wDetachTimeOut;

        /// <summary>Transfer size for download/upload.</summary>
        public ushort wTransferSize;

        /// <summary>Binary coded(BCD) value of DFU version.</summary>
        public ushort bcdDFUVersion;
    }
}