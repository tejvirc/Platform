namespace Aristocrat.Monaco.G2S.Options
{
    using Accounting.Contracts;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Kernel.Contracts;

    /// <inheritdoc />
    public class VoucherDeviceOptions : BaseDeviceOptions
    {
        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Voucher;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetPropertyValues(optionConfigValues);

            SetRedeemPrefix(optionConfigValues);

            SetVoucherTitles(optionConfigValues);
        }

        private void SetVoucherTitles(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleCashParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleCash,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleCashParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitlePromoParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitlePromo,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitlePromoParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleNonCashParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleNonCash,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleNonCashParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleLargeWinParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleLargeWin,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleLargeWinParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleBonusCashParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleBonusCash,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleBonusCashParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleBonusPromoParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleBonusPromo,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleBonusPromoParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleBonusNonCashParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleBonusNonCash,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleBonusNonCashParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleWatCashParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleWatCash,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleWatCashParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleWatPromoParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleWatPromo,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleWatPromoParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.TitleWatNonCashParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.TicketTitleWatNonCash,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.TitleWatNonCashParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.ExpireCashPromoParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.VoucherOutExpirationDays,
                    optionConfigValues.Int32Value(G2SParametersNames.VoucherDevice.ExpireCashPromoParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.ExpireNonCashParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.VoucherOutNonCashExpirationDays,
                    optionConfigValues.Int32Value(G2SParametersNames.VoucherDevice.ExpireNonCashParameterName));
            }
        }

        private void SetPropertyValues(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.AllowVoucherIssueParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.VoucherOut,
                    optionConfigValues.BooleanValue(G2SParametersNames.VoucherDevice.AllowVoucherIssueParameterName),
                    true);
            }

            if (optionConfigValues.HasKey(G2SParametersNames.VoucherDevice.AllowNonCashableOutParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.VoucherOutNonCash,
                    optionConfigValues.BooleanValue(G2SParametersNames.VoucherDevice.AllowNonCashableOutParameterName),
                    true);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.AllowVoucherRedeemParameterName))
            {
                PropertiesManager.SetProperty(
                    PropertyKey.VoucherIn,
                    optionConfigValues.BooleanValue(G2SParametersNames.VoucherDevice.AllowVoucherRedeemParameterName),
                    true);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.PropertyNameParameterName))
            {
                PropertiesManager.SetProperty(
                    PropertyKey.TicketTextLine1,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.PropertyNameParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.PropertyLine1ParameterName))
            {
                PropertiesManager.SetProperty(
                    PropertyKey.TicketTextLine2,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.PropertyLine1ParameterName));
            }

            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.PropertyLine2ParameterName))
            {
                PropertiesManager.SetProperty(
                    PropertyKey.TicketTextLine3,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.PropertyLine2ParameterName));
            }
        }

        private void SetRedeemPrefix(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.VoucherDevice.RedeemPrefixParameterName))
            {
                PropertiesManager.SetProperty(
                    AccountingConstants.RedeemText,
                    optionConfigValues.StringValue(G2SParametersNames.VoucherDevice.RedeemPrefixParameterName),
                    true);
            }
        }
    }
}