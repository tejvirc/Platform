namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Handlers.OptionConfig.Builders;

    public class SpcDeviceOptions : BaseDeviceOptions
    {
        private static readonly string[] Options =
        {
            OptionConstants.ProtocolOptionsId,
            OptionConstants.LevelTableOptionsId,
            OptionConstants.GameConfigTableOptionsId
        };

        private static readonly Dictionary<string, OptionDataType> OptionsValues = new Dictionary
                <string, OptionDataType>()
            .AddValues(AddProtocolOptionsTypes())
            .AddValues(AddSpcOptionsTypes());

        /// <inheritdoc />
        protected override IReadOnlyDictionary<string, OptionDataType> SupportedValues => OptionsValues;

        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Spc;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetValues(optionConfigValues, device as SpcDevice);
        }

        private static IEnumerable<Tuple<string, OptionDataType>> AddSpcOptionsTypes()
        {
            return new List<Tuple<string, OptionDataType>>
            {
                new Tuple<string, OptionDataType>(OptionConstants.LevelTableOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(OptionConstants.GameConfigTableOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.ConfigDateTimeParameterName,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.ConfigCompleteParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.RestartStatusParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.UseDefaultConfigParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.RequiredForPlayParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.ControllerTypeParameterName,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.LevelIdParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.ResetAmountParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.MaxLevelAmountParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.ContribPercentParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.RoundingEnabledParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.MystMinParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.MystMaxParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.GamePlayIdParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.WinLevelIndexParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.PaytableIdParameterName,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.ThemeIdParameterName,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.SpcDevice.DenomIdParameterName,
                    OptionDataType.Integer)
            };
        }


        private void SetValues(DeviceOptionConfigValues optionConfigValues, SpcDevice device)
        {
            if (device == null)
            {
                return;
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.ControllerTypeParameterName))
            {
                device.ControllerType =
                    optionConfigValues.StringValue(G2SParametersNames.SpcDevice.ControllerTypeParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.LevelIdParameterName))
            {
                device.LevelId =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.LevelIdParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.ResetAmountParameterName))
            {
                device.ResetAmount =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.ResetAmountParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.MaxLevelAmountParameterName))
            {
                device.MaxLevelAmount =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.MaxLevelAmountParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.ContribPercentParameterName))
            {
                device.ContribPercent =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.ContribPercentParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.RoundingEnabledParameterName))
            {
                device.RoundingEnabled =
                    optionConfigValues.BooleanValue(G2SParametersNames.SpcDevice.RoundingEnabledParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.MystMinParameterName))
            {
                device.MysteryMinimum =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.MystMinParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.MystMaxParameterName))
            {
                device.MysteryMaximum =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.MystMaxParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.GamePlayIdParameterName))
            {
                device.GamePlayId =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.GamePlayIdParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.WinLevelIndexParameterName))
            {
                device.WinLevelIndex =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.WinLevelIndexParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.PaytableIdParameterName))
            {
                device.PaytableId =
                    optionConfigValues.StringValue(G2SParametersNames.SpcDevice.PaytableIdParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.ThemeIdParameterName))
            {
                device.ThemeId =
                    optionConfigValues.StringValue(G2SParametersNames.SpcDevice.ThemeIdParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.SpcDevice.DenomIdParameterName))
            {
                device.DenomId =
                    optionConfigValues.Int32Value(G2SParametersNames.SpcDevice.DenomIdParameterName);
            }
        }

    }
}
