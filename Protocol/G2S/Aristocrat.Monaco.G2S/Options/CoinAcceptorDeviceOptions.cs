namespace Aristocrat.Monaco.G2S.Options
{
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;

    /// <inheritdoc />
    public class CoinAcceptorDeviceOptions : BaseDeviceOptions
    {
        private const string CurrencyIdParameterName = "G2S_currencyId";
        private const string DenomIdParameterName = "G2S_denomId";
        private const string TokenParameterName = "G2S_token";
        private const string BaseCashableAmtParameterName = "G2S_baseCashableAmt";
        private const string CoinActiveParameterName = "G2S_coinActive";
        private const string BasePromoAmtParameterName = "G2S_basePromoAmt";
        private const string BaseNonCashAmtParameterName = "G2S_baseNonCashAmt";

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.CoinAcceptor;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetCurrencyId(optionConfigValues);
            SetDenomId(optionConfigValues);
            SetToken(optionConfigValues);
            SetBaseCashableAmt(optionConfigValues);
            SetCoinActive(optionConfigValues);
            SetBasePromoAmt(optionConfigValues);
            SetBaseNonCashAmt(optionConfigValues);
        }

        private void SetCurrencyId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(CurrencyIdParameterName))
            {
            }
        }

        private void SetDenomId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(DenomIdParameterName))
            {
            }
        }

        private void SetToken(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(TokenParameterName))
            {
            }
        }

        private void SetBaseCashableAmt(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(BaseCashableAmtParameterName))
            {
            }
        }

        private void SetCoinActive(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(CoinActiveParameterName))
            {
            }
        }

        private void SetBasePromoAmt(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(BasePromoAmtParameterName))
            {
            }
        }

        private void SetBaseNonCashAmt(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(BaseNonCashAmtParameterName))
            {
            }
        }
    }
}