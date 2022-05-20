namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Kernel;

    /// <inheritdoc />
    public class CoinAcceptorDeviceOptionBuilder : BaseDeviceOptionBuilder<CoinAcceptorDevice>
    {
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorDeviceOptionBuilder" /> class.
        /// </summary>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        public CoinAcceptorDeviceOptionBuilder(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.CoinAcceptor;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            CoinAcceptorDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_coinAcceptorOptions",
                optionGroupName = "G2S Coin Acceptor Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for coin acceptor devices",
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.CoinDataTableOptionsId, parameters))
            {
                items.Add(BuildCoinDataTable(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildCoinDataTable(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CurrencyIdParameterName,
                    ParamName = "Currency ID",
                    ParamHelp = "Currency identifier",
                    ParamCreator = () => new stringParameter { maxLen = 3 }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CoinAcceptorDevice.DenomIdParameterName,
                    ParamName = "Denomination ID",
                    ParamHelp = "Denomination identifier",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CoinAcceptorDevice.TokenParameterName,
                    ParamName = "Coin is a Token",
                    ParamHelp = "This coin is a token",
                    ParamCreator = () => new booleanParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CoinAcceptorDevice.BaseCashableAmtParameterName,
                    ParamName = "Cashable Exchange Value Per Unit",
                    ParamHelp = "Actual cashable value of this coin (in millicents)",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CoinAcceptorDevice.BasePromoAmtParameterName,
                    ParamName = "Promotional Exchange Value Per Unit",
                    ParamHelp = "Actual promotional value of this coin (in millicents)",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CoinAcceptorDevice.BaseNonCashAmtParameterName,
                    ParamName = "Non-cashable Exchange Value Per Unit",
                    ParamHelp = "Actual non-cashable value of this coin (in millicents)",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CoinAcceptorDevice.CoinActiveParameterName,
                    ParamName = "Coin Activated",
                    ParamHelp = "This coin is active (can be accepted)",
                    ParamCreator = () => new booleanParameter()
                }
            };

            // TODO:  This hardcoded just for Quebec (we don't have a coin acceptor, but the host expects some data here)
            var sampleCoins = new[] { 25000, 100000, 200000 };

            var currencyId = _properties.GetValue(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId);

            var currentValues = sampleCoins.Select(c => CoinDataValue(c, currencyId));

            var defaultValues = sampleCoins.Select(c => CoinDataValue(c, currencyId));

            return BuildOptionItemForTable(
                OptionConstants.CoinDataTableOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_coinData",
                "Coin Data",
                "Information on one coin that can be accepted by the device",
                parameters,
                currentValues,
                defaultValues,
                includeDetails);
        }

        private complexValue CoinDataValue(int denom, string currencyId)
        {
            return new complexValue
            {
                paramId = "G2S_coinData",
                Items = new object[]
                {
                    new booleanValue1
                    {
                        paramId = G2SParametersNames.CoinAcceptorDevice.TokenParameterName,
                        Value = false
                    },
                    new booleanValue1
                    {
                        paramId = G2SParametersNames.CoinAcceptorDevice.CoinActiveParameterName,
                        Value = true
                    },
                    new stringValue1
                    {
                        paramId = G2SParametersNames.CurrencyIdParameterName,
                        Value = currencyId
                    },
                    new integerValue1
                    {
                        paramId = G2SParametersNames.CoinAcceptorDevice.DenomIdParameterName,
                        Value = denom
                    },
                    new integerValue1
                    {
                        paramId = G2SParametersNames.CoinAcceptorDevice.BaseCashableAmtParameterName,
                        Value = denom
                    },
                    new integerValue1
                    {
                        paramId = G2SParametersNames.CoinAcceptorDevice.BasePromoAmtParameterName,
                        Value = 0
                    },
                    new integerValue1
                    {
                        paramId = G2SParametersNames.CoinAcceptorDevice.BaseNonCashAmtParameterName,
                        Value = 0
                    }
                }
            };
        }
    }
}