namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using Accounting.Contracts;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using System.Collections.Generic;
    using System.Linq;
    using Handpay;
    using Services;

    /// <summary>
    /// </summary>
    public class HandpayDeviceOptionBuilder : BaseDeviceOptionBuilder<HandpayDevice>
    {
        private readonly IHandpayProperties _properties;

        public HandpayDeviceOptionBuilder(IHandpayProperties properties)
        {
            _properties = properties;
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.HandPay;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            HandpayDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_handpayOptions",
                optionGroupName = "G2S Handpay Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for the handpay device",
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.HandPayOptionsId, parameters))
            {
                items.Add(BuildHandPayOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.LocalKeyOffOptionsId, parameters))
            {
                items.Add(BuildLocalKeyOffOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.PartialHandPayOptionsId, parameters))
            {
                items.Add(BuildPartialHandPayOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.HandPayReceiptOptionsId, parameters))
            {
                items.Add(BuildHandPayReceiptOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.UsePlayerIdReaderOptionsOptionsId, parameters))
            {
                items.Add(BuildUsePlayerIdReaderOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildHandPayOptions(HandpayDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnabledLocalHandpay,
                    ParamName = "Local Handpays When Enabled",
                    ParamHelp = "Local handpays permitted when EGM is enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = _properties.EnabledLocalHandpay
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnabledLocalCredit,
                    ParamName = "Local Key-Off to Credit Meter When Enabled",
                    ParamHelp = "Local key-off to credit meter permitted when EGM is enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.EnabledLocalCredit
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnabledLocalVoucher,
                    ParamName = "Local Key-Off to Voucher When Enabled",
                    ParamHelp = "Local key-off to voucher permitted when EGM is enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.EnabledLocalVoucher
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnabledLocalWat,
                    ParamName = "Local Key-Off to WAT When Enabled",
                    ParamHelp = "Local key-off to WAT permitted when EGM is enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.EnabledLocalWat
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnabledRemoteHandpay,
                    ParamName = "Remote Handpays When Enabled",
                    ParamHelp = "Remote handpays permitted when EGM is enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.EnabledRemoteHandpay
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnabledRemoteCredit,
                    ParamName = "Remote Key-Off to Credit Meter When Enabled",
                    ParamHelp = "Remote key-off to credit meter permitted when EGM is enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.EnabledRemoteCredit
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnabledRemoteVoucher,
                    ParamName = "Remote Key-Off to Voucher When Enabled",
                    ParamHelp = "Remote key-off to voucher permitted when EGM is enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.EnabledRemoteVoucher
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnabledRemoteWat,
                    ParamName = "Remote Key-Off to WAT When Enabled",
                    ParamHelp = "Remote key-off to WAT permitted when EGM is enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.EnabledRemoteWat
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.DisabledLocalHandpay,
                    ParamName = "Local Handpays When Disabled",
                    ParamHelp = "Local handpays permitted when EGM is disabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = _properties.DisabledLocalHandpay
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.DisabledLocalCredit,
                    ParamName = "Local Key-Off to Credit Meter When Disabled",
                    ParamHelp = "Local key-off to credit meter permitted when EGM is disabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.DisabledLocalCredit
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.DisabledLocalVoucher,
                    ParamName = "Local Key-Off to Voucher When Disabled",
                    ParamHelp = "Local key-off to voucher permitted when EGM is disabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.DisabledLocalVoucher
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.DisabledLocalWat,
                    ParamName = "Local Key-Off to WAT When Disabled",
                    ParamHelp = "Local key-off to WAT permitted when EGM is disabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.DisabledLocalWat
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.DisabledRemoteHandpay,
                    ParamName = "Remote Handpays When Disabled",
                    ParamHelp = "Remote handpays permitted when EGM is disabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.DisabledRemoteHandpay
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.DisabledRemoteCredit,
                    ParamName = "Remote Key-Off to Credit Meter When Disabled",
                    ParamHelp = "Remote key-off to credit meter permitted when EGM is disabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.DisabledRemoteCredit
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.DisabledRemoteVoucher,
                    ParamName = "Remote Key-Off to Voucher When Disabled",
                    ParamHelp = "Remote key-off to voucher permitted when EGM is disabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.DisabledRemoteVoucher
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.DisabledRemoteWat,
                    ParamName = "Remote Key-Off to WAT When Disabled",
                    ParamHelp = "Remote key-off to WAT permitted when EGM is disabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.DisabledRemoteWat
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.MixCreditTypes,
                    ParamName = "Mix Credit Types",
                    ParamHelp = "Mix multiple credit types in a single handpay",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = _properties.MixCreditTypes
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.RequestNonCash,
                    ParamName = "Handpay Non-Cashable Credits",
                    ParamHelp = "Include non-cashable credits in handpay requests",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = _properties.RequestNonCash
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.CombineCashableOut,
                    ParamName = "Combine Cashable Types",
                    ParamHelp = "Combine cashable and promo cashable on one handpay",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = _properties.CombineCashableOut
                }
            };

            return BuildOptionItem(
                OptionConstants.HandPayOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                G2SParametersNames.HandpayDevice.HandpayParams,
                "Handpay Parameters",
                "Configuration settings for this handpay device",
                parameters,
                includeDetails);
        }

        private optionItem BuildLocalKeyOffOptions(HandpayDevice device, bool includeDetails)
        {
            var parameter = new ParameterDescription
            {
                ParamId = OptionConstants.LocalKeyOffOptionsId,
                ParamName = "Local Key-Off Default",
                ParamHelp = "Local key-off methods prior to host authorization.",
                ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 32 },
                ValueCreator = () => new stringValue1(),
                DefaultValue = Constants.DefaultLocalKeyoff,
                Value = _properties.LocalKeyOff.ToG2SEnum()
            };
            
            return BuildOptionItemOfNonComplexType(
                OptionConstants.LocalKeyOffOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                OptionConstants.LocalKeyOffOptionsId,
                "Local Key-Off Default",
                "Local key-off methods prior to host authorization",
                parameter,
                includeDetails);
        }

        private optionItem BuildPartialHandPayOptions(HandpayDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.PartialHandpays,
                    ParamName = "Partial Handpays",
                    ParamHelp =
                        "Indicates whether requests for partial handpays should be accepted for cashable and promotional credits",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.PartialHandpays
                }
            };

            return BuildOptionItem(
                OptionConstants.PartialHandPayOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                G2SParametersNames.HandpayDevice.PartialHandpayParams,
                "Partial Handpay Parameters",
                "Parameters for enabling partial handpays",
                parameters,
                includeDetails);
        }

        private optionItem BuildHandPayReceiptOptions(HandpayDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.EnableReceipts,
                    ParamName = "Enable non-validated handpay receipts",
                    ParamHelp = "Indicates whether nonvalidated handpay receipts are enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.EnableReceipts
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.TitleJackpotReceipt,
                    ParamName = "Jackpot receipt title",
                    ParamHelp = "Title printed on game win and bonus pay receipts",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 32 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = AccountingConstants.DefaultTicketLine,
                    Value = Truncate(
                    _properties.TitleJackpotReceipt,
                    Constants.VoucherTitle40),
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.TitleCancelReceipt,
                    ParamName = "Cancel credit receipt title",
                    ParamHelp = "Title printed on cancel credit receipts",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 32 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = AccountingConstants.DefaultTicketLine,
                    Value = Truncate(
                        _properties.TitleCancelReceipt,
                        Constants.VoucherTitle40),
                }
            };

            return BuildOptionItem(
                OptionConstants.HandPayReceiptOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                G2SParametersNames.HandpayDevice.HandpayReceiptParams,
                "Non-Validated Handpay Receipt Parameters",
                "Parameters for enabling non-validated handpay receipts",
                parameters.OrderBy(x => x.ParamName),
                includeDetails);
        }

        private optionItem BuildUsePlayerIdReaderOptions(HandpayDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.HandpayDevice.UsePlayerIdReader,
                    ParamName = "Use Player ID Reader",
                    ParamHelp =
                        "Indicates whether the ID reader associated with the currently active player session should be used",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = _properties.UsePlayerIdReader
                }
            };

            return BuildOptionItem(
                OptionConstants.UsePlayerIdReaderOptionsOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                G2SParametersNames.HandpayDevice.UsePlayerIdReaderParams,
                "Use Player ID Reader",
                "Parameters associated with Use-Player-ID-Reader option",
                parameters,
                includeDetails);
        }

        private static string Truncate(string source, int length)
        {
            if (source.Length > length)
            {
                source = source.Substring(0, length);
            }

            return source;
        }
    }
}