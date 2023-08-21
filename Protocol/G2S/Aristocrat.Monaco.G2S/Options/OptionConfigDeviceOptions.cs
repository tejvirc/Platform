namespace Aristocrat.Monaco.G2S.Options
{
    using System.Collections.Generic;
    using Data.Model;
    using Handlers.OptionConfig.Builders;

    /// <inheritdoc />
    public class OptionConfigDeviceOptions : BaseDeviceOptions
    {
        private static readonly string[] Options =
        {
            OptionConstants.ProtocolOptionsId,
            OptionConstants.ProtocolAdditionalOptionsId
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
            return deviceClass == DeviceClass.OptionConfig;
        }
    }
}