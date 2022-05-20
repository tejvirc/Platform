namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Handlers.OptionConfig.Builders;
    using Handlers.Handpay;

    /// <inheritdoc />
    public class HandpayDeviceOptions : DeviceOptions<IHandpayDevice>
    {
        private static readonly string[] Options =
        {
            OptionConstants.ProtocolOptionsId,
            OptionConstants.ProtocolAdditionalOptionsId,
            OptionConstants.HandPayOptionsId,
            OptionConstants.LocalKeyOffOptionsId,
            OptionConstants.PartialHandPayOptionsId,
            OptionConstants.HandPayReceiptOptionsId,
            OptionConstants.UsePlayerIdReaderOptionsOptionsId
        };

        private static readonly Dictionary<string, OptionDataType> OptionsValues = new Dictionary
                <string, OptionDataType>()
            .AddValues(AddProtocolOptionsTypes())
            .AddValues(AddProtocolOptions3Types())
            .AddValues(AddHandPayOptionsTypes());

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayDeviceOptions" /> class.
        /// </summary>
        public HandpayDeviceOptions()
            : base(DeviceClass.HandPay)
        {
            
        }

        /// <inheritdoc />
        protected override IEnumerable<string> SupportedOptions => Options;

        /// <inheritdoc />
        protected override IReadOnlyDictionary<string, OptionDataType> SupportedValues => OptionsValues;

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetValues(optionConfigValues, device.Id);
        }

        private static IEnumerable<Tuple<string, OptionDataType>> AddHandPayOptionsTypes()
        {
            return new List<Tuple<string, OptionDataType>>
            {
                new Tuple<string, OptionDataType>(OptionConstants.HandPayOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(OptionConstants.LocalKeyOffOptionsId, OptionDataType.String),
                new Tuple<string, OptionDataType>(OptionConstants.PartialHandPayOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(OptionConstants.HandPayReceiptOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(
                    OptionConstants.UsePlayerIdReaderOptionsOptionsId,
                    OptionDataType.Complex),
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
                    G2SParametersNames.HandpayDevice.EnabledLocalHandpay,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.EnabledLocalCredit,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.EnabledLocalVoucher,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.EnabledLocalWat,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.EnabledRemoteHandpay,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.EnabledRemoteCredit,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.EnabledRemoteVoucher,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.EnabledRemoteWat,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.DisabledLocalHandpay,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.DisabledLocalCredit,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.DisabledLocalVoucher,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.DisabledLocalWat,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.DisabledRemoteHandpay,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.DisabledRemoteCredit,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.DisabledRemoteVoucher,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.DisabledRemoteWat,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.MixCreditTypes,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.RequestNonCash,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.CombineCashableOut,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.PartialHandpays,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.EnableReceipts,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.TitleJackpotReceipt,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.TitleCancelReceipt,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.HandpayDevice.UsePlayerIdReader,
                    OptionDataType.Boolean)
            };
        }

        private void SetValues(DeviceOptionConfigValues optionConfigValues, int deviceId)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledLocalHandpay))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledLocalHandpay,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledLocalHandpay));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledLocalCredit))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledLocalCredit,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledLocalCredit));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledLocalVoucher))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledLocalVoucher,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledLocalVoucher));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledLocalWat))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledLocalWat,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledLocalWat));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledRemoteHandpay))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledRemoteHandpay,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledRemoteHandpay));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledRemoteCredit))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledRemoteCredit,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledRemoteCredit));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledRemoteCredit))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledRemoteCredit,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledRemoteCredit));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledRemoteVoucher))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledRemoteVoucher,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledRemoteVoucher));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnabledRemoteWat))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnabledRemoteWat,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnabledRemoteWat));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.DisabledLocalHandpay))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.DisabledLocalHandpay,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.DisabledLocalHandpay));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.DisabledLocalCredit))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.DisabledLocalCredit,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.DisabledLocalCredit));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.DisabledLocalVoucher))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.DisabledLocalVoucher,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.DisabledLocalVoucher));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.DisabledLocalWat))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.DisabledLocalWat,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.DisabledLocalWat));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.DisabledRemoteHandpay))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.DisabledRemoteHandpay,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.DisabledRemoteHandpay));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.DisabledRemoteCredit))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.DisabledRemoteCredit,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.DisabledRemoteCredit));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.DisabledRemoteVoucher))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.DisabledRemoteVoucher,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.DisabledRemoteVoucher));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.DisabledRemoteWat))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.DisabledRemoteWat,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.DisabledRemoteWat));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.MixCreditTypes))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.MixCreditTypes,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.MixCreditTypes));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.RequestNonCash))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.RequestNonCash,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.RequestNonCash));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.CombineCashableOut))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.CombineCashableOut,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.CombineCashableOut));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.LocalKeyOff))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.LocalKeyOff,
                    optionConfigValues.StringValue(G2SParametersNames.HandpayDevice.LocalKeyOff).LocalKeyOffFromG2SString());
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.PartialHandpays))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.PartialHandpays,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.PartialHandpays));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.EnableReceipts))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.EnableReceipts,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.EnableReceipts));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.TitleJackpotReceipt))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TitleJackpotReceipt,
                    optionConfigValues.StringValue(G2SParametersNames.HandpayDevice.TitleJackpotReceipt));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.TitleCancelReceipt))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TitleCancelReceipt,
                    optionConfigValues.StringValue(G2SParametersNames.HandpayDevice.TitleCancelReceipt));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.HandpayDevice.UsePlayerIdReader))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.UsePlayerIdReader,
                    optionConfigValues.BooleanValue(G2SParametersNames.HandpayDevice.UsePlayerIdReader));
            }
        }
    }
}