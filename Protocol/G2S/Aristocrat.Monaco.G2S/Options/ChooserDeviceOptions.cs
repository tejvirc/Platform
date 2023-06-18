namespace Aristocrat.Monaco.G2S.Options
{
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Gaming.Contracts;
    using Handlers.OptionConfig.Builders;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using Constants = Aristocrat.G2S.Client.Devices.v21.G2SParametersNames.ChooserDevice;

    public class ChooserDeviceOptions : BaseDeviceOptions
    {
        private static readonly string[] Options =
        {
            OptionConstants.GameComboTagDataTable
        };

        private static readonly Dictionary<string, OptionDataType> OptionsValues = new Dictionary
            <string, OptionDataType>().AddValues(AddChooserOptionsTypes());

        /// <inheritdoc />
        protected override IEnumerable<string> SupportedOptions => Options;

        /// <inheritdoc />
        protected override IReadOnlyDictionary<string, OptionDataType> SupportedValues => OptionsValues;

        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Chooser;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetChooserDevice(optionConfigValues);
        }

        private void SetChooserDevice(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasTableValue(Constants.GameComboPositionParameterName))
            {
                var table = optionConfigValues.GetTableValue(Constants.GameComboPositionParameterName);

                var order = ServiceManager.GetInstance().TryGetService<IGameOrderSettings>();

                foreach (var tableRow in table)
                {
                    if (tableRow.HasValue(Constants.PositionParameterName))
                    {
                        var id = tableRow.GetDeviceOptionConfigValue(Constants.ThemeIdParameterName).StringValue();
                        var position = tableRow.GetDeviceOptionConfigValue(Constants.PositionParameterName).Int32Value();

                        if (!string.IsNullOrEmpty(id))
                            order.UpdateIconPositionPriority(id, position);
                    }
                }
            }

            if (optionConfigValues.HasTableValue(Constants.GameTagDataParameterName))
            {
                var table = optionConfigValues.GetTableValue(Constants.GameTagDataParameterName);

                var provider = ServiceManager.GetInstance().TryGetService<IGameProvider>();

                // Since there doesn't seem to be a way to tell if tags have been replaced, just reset tags for all games
                var tags = new Dictionary<int, List<string>>();
                foreach (var tableRow in table)
                {
                    if (tableRow.HasValue(Constants.GameTagParameterName))
                    {
                        var id = tableRow.GetDeviceOptionConfigValue(Constants.GamePlayIdParameterName).Int32Value();
                        var tag = tableRow.GetDeviceOptionConfigValue(Constants.GameTagParameterName).StringValue()
                            .ToInternalGameTagString();

                        if (id > 0)
                        {
                            if (tags.ContainsKey(id))
                                tags[id].Add(tag);
                            else
                                tags.Add(id, new List<string> { tag });
                        }
                    }
                }

                foreach (var t in tags)
                {
                    provider.SetGameTags(t.Key, t.Value);
                }
            }
        }

        private static IEnumerable<Tuple<string, OptionDataType>> AddChooserOptionsTypes()
        {
            return new List<Tuple<string, OptionDataType>>
            {
                new Tuple<string, OptionDataType>(Constants.GameTagDataParameterName, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(Constants.DenomIdParameterName, OptionDataType.Integer),
                new Tuple<string, OptionDataType>(Constants.GameTagParameterName, OptionDataType.String),
                new Tuple<string, OptionDataType>(Constants.GamePlayIdParameterName, OptionDataType.Integer),
                new Tuple<string, OptionDataType>(Constants.PaytableIdParameterName, OptionDataType.String),
                new Tuple<string, OptionDataType>(Constants.ThemeIdParameterName, OptionDataType.String),
            };
        }
    }
}
