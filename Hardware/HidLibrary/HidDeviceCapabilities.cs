namespace Vgt.Client12.Hardware.HidLibrary
{
    public class HidDeviceCapabilities
    {
        internal HidDeviceCapabilities(NativeMethods.HIDP_CAPS capabilities)
        {
            Usage = capabilities.Usage;
            UsagePage = capabilities.UsagePage;
            InputReportByteLength = capabilities.InputReportByteLength;
            OutputReportByteLength = capabilities.OutputReportByteLength;
            FeatureReportByteLength = capabilities.FeatureReportByteLength;
            Reserved = capabilities.Reserved;
            NumberLinkCollectionNodes = capabilities.NumberLinkCollectionNodes;
            NumberInputButtonCaps = capabilities.NumberInputButtonCaps;
            NumberInputValueCaps = capabilities.NumberInputValueCaps;
            NumberInputDataIndices = capabilities.NumberInputDataIndices;
            NumberOutputButtonCaps = capabilities.NumberOutputButtonCaps;
            NumberOutputValueCaps = capabilities.NumberOutputValueCaps;
            NumberOutputDataIndices = capabilities.NumberOutputDataIndices;
            NumberFeatureButtonCaps = capabilities.NumberFeatureButtonCaps;
            NumberFeatureValueCaps = capabilities.NumberFeatureValueCaps;
            NumberFeatureDataIndices = capabilities.NumberFeatureDataIndices;
        }

        public short Usage { get; }
        public short UsagePage { get; }
        public short InputReportByteLength { get; }
        public short OutputReportByteLength { get; }
        public short FeatureReportByteLength { get; }
        public short[] Reserved { get; }
        public short NumberLinkCollectionNodes { get; }
        public short NumberInputButtonCaps { get; }
        public short NumberInputValueCaps { get; }
        public short NumberInputDataIndices { get; }
        public short NumberOutputButtonCaps { get; }
        public short NumberOutputValueCaps { get; }
        public short NumberOutputDataIndices { get; }
        public short NumberFeatureButtonCaps { get; }
        public short NumberFeatureValueCaps { get; }
        public short NumberFeatureDataIndices { get; }
    }
}