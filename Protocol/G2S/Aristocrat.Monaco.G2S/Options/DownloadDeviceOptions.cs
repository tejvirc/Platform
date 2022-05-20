namespace Aristocrat.Monaco.G2S.Options
{
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Handlers.OptionConfig.Builders;
    using Kernel;

    /// <inheritdoc />
    public class DownloadDeviceOptions : BaseDeviceOptions
    {
        private static readonly string[] Options =
        {
            OptionConstants.ProtocolOptionsId, OptionConstants.ProtocolAdditionalOptionsId
        };

        private static readonly Dictionary<string, OptionDataType> OptionsValues = new Dictionary
            <string, OptionDataType>().AddValues(AddProtocolOptionsTypes()).AddValues(AddProtocolOptions3Types());

        /// <inheritdoc />
        protected override IEnumerable<string> SupportedOptions => Options;

        /// <inheritdoc />
        protected override IReadOnlyDictionary<string, OptionDataType> SupportedValues => OptionsValues;

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Download;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.DownloadDevice.DownloadEnabledParameterName))
            {
                // This effectively overrides the host when this forcibly disabled by configuration
                if (PropertiesManager.GetValue(ApplicationConstants.ReadOnlyMediaRequired, false) &&
                    device is IDownloadDevice download)
                {
                    download.DownloadEnabled = false;
                }
            }
        }
    }
}