namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using Constants = G2S.Constants;

    /// <inheritdoc />
    public class VoucherDeviceOptionBuilder : BaseDeviceOptionBuilder<VoucherDevice>
    {
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherDeviceOptionBuilder" /> class.
        /// </summary>
        public VoucherDeviceOptionBuilder()
        {
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.Voucher;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            VoucherDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_voucherOptions", optionGroupName = "G2S Voucher Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                var additionalParameters = BuildAdditionalProtocolParameters(device);

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for voucher devices",
                        additionalParameters,
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.VoucherOptionsId, parameters))
            {
                items.Add(BuildVoucherOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.VoucherTextFieldsOptionId, parameters))
            {
                items.Add(BuildVoucherTextFieldsOption(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.VoucherOptions2Id, parameters))
            {
                items.Add(BuildVoucherOptions2(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.VoucherLimitsOptionId, parameters))
            {
                items.Add(BuildVoucherLimitsOption(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.VoucherOptions3Id, parameters))
            {
                items.Add(BuildVoucherOptions3(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private List<ParameterDescription> BuildAdditionalProtocolParameters(IVoucherDevice device)
        {
            return new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.TimeToLiveParameterName,
                    ParamName = "Time To Live",
                    ParamHelp = "Time to live value for requests (in milliseconds)",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.TimeToLive,
                    DefaultValue = (int)Aristocrat.G2S.Client.Constants.DefaultTimeout.TotalMilliseconds
                }
            };
        }

        private optionItem BuildVoucherOptions(IVoucherDevice device, bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.IdReaderIdParameterName,
                    ParamName = "ID Reader to Use",
                    ParamHelp = "ID reader to use for this voucher device",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.IdReaderId,
                    DefaultValue = VoucherDevice.DefaultIdReaderId
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.CombineCashableOutParameterName,
                    ParamName = "Combine Cashable Credit Types.",
                    ParamHelp = "Combine cashable and promo credit in a single voucher",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.CombineCashableOut,
                    DefaultValue = VoucherDevice.DefaultCombineCashableOut
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.AllowNonCashableOutParameterName,
                    ParamName = "Allow Non-Cashable Out",
                    ParamHelp = "Allow vouchers for non-cashable credits",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = _propertiesManager.GetValue(AccountingConstants.VoucherOutNonCash, false),
                    DefaultValue = VoucherDevice.DefaultAllowNonCashOut
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.MaxValidationIdsParameterName,
                    ParamName = "Maximum Validation IDs",
                    ParamHelp = "Maximum validation IDs EGM may buffer",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.MaxValueIds,
                    DefaultValue = VoucherDevice.DefaultMaxValIds
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.MinLevelValidationIdsParameterName,
                    ParamName = "Minimum Level for Validation IDs",
                    ParamHelp = "Minimum validation IDs EGM must maintain",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.MinLevelValueIds,
                    DefaultValue = VoucherDevice.DefaultMinLevelValIds
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.ValidationIdListRefreshParameterName,
                    ParamName = "Validation ID Refresh Time",
                    ParamHelp = "Maximum time before EGM requests validation ID list update.",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.ValueIdListRefresh,
                    DefaultValue = VoucherDevice.DefaultValIdListRefresh
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.ValidationIdListLifeParameterName,
                    ParamName = "Validation ID List Life",
                    ParamHelp = "Maximum life of validation IDs without host intervention.",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.ValueIdListLife,
                    DefaultValue = VoucherDevice.DefaultValIdListLife
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.VoucherHoldTimeParameterName,
                    ParamName = "Maximum Voucher Hold Time",
                    ParamHelp = "Maximum time EGM should escrow a voucher",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.VoucherHoldTime,
                    DefaultValue = VoucherDevice.DefaultVoucherHoldTime
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.PrintOfflineParameterName,
                    ParamName = "Print Offline",
                    ParamHelp = "Allow vouchers to be printed while host is offline",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = device.PrintOffLine,
                    DefaultValue = VoucherDevice.DefaultPrintOffLine
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.ExpireCashPromoParameterName,
                    ParamName = "Expire Days Cash Promo",
                    ParamHelp = "Number of days before cashable and promo vouchers expire.",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.ExpireCashPromo != Aristocrat.G2S.Client.Constants.ExpirationNotSet
                        ? device.ExpireCashPromo
                        : _propertiesManager.GetProperty(
                            AccountingConstants.VoucherOutExpirationDays,
                            VoucherDevice.DefaultExpireCashPromo),
                    DefaultValue = VoucherDevice.DefaultExpireCashPromo
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.PrintExpirationCashPromoParameterName,
                    ParamName = "Print Cash Promo Expirations",
                    ParamHelp = "Print expiration on cashable and promo vouchers",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = device.PrintExpirationCashPromo,
                    DefaultValue = VoucherDevice.DefaultPrintExpCashPromo
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.ExpireNonCashParameterName,
                    ParamName = "Expire Non-Cashable",
                    ParamHelp = "Default number of days before non-cashable vouchers expire.",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.ExpireNonCash != Aristocrat.G2S.Client.Constants.ExpirationNotSet
                        ? device.ExpireNonCash
                        : _propertiesManager.GetProperty(
                            AccountingConstants.VoucherOutNonCashExpirationDays,
                            VoucherDevice.DefaultExpireNonCash),
                    DefaultValue = VoucherDevice.DefaultExpireNonCash
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.PrintExpireNonCashParameterName,
                    ParamName = "Print Non-Cashable Expirations",
                    ParamHelp = "Print expiration on non-cashable vouchers",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = device.PrintExpirationNonCash,
                    DefaultValue = VoucherDevice.DefaultPrintExpNonCash
                }
            };

            return BuildOptionItem(
                OptionConstants.VoucherOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_voucherParams",
                "Voucher Options",
                "Configuration parameters for this voucher device",
                parameters,
                includeDetails);
        }

        private optionItem BuildVoucherTextFieldsOption(bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.PropertyNameParameterName,
                    ParamName = "Property Name",
                    ParamHelp = "Name of the property",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 40 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(PropertyKey.TicketTextLine1, string.Empty),
                        Constants.VoucherTitle40),
                    DefaultValue = AccountingConstants.DefaultTicketLine
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.PropertyLine1ParameterName,
                    ParamName = "Property Line 1",
                    ParamHelp = "Property address line 1",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 40 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(PropertyKey.TicketTextLine2, string.Empty),
                        Constants.VoucherTitle40),
                    DefaultValue = AccountingConstants.DefaultTicketLine
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.PropertyLine2ParameterName,
                    ParamName = "Property Line 2",
                    ParamHelp = "Property address line 2",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 40 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(PropertyKey.TicketTextLine3, string.Empty),
                        Constants.VoucherTitle40),
                    DefaultValue = AccountingConstants.DefaultTicketLine
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleCashParameterName,
                    ParamName = "Cashable Title",
                    ParamHelp = "Title printed on vouchers for cashable credits",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(
                            AccountingConstants.TicketTitleCash,
                            AccountingConstants.DefaultCashoutTicketTitle),
                        Constants.VoucherTitle16),
                    DefaultValue = AccountingConstants.DefaultCashoutTicketTitle
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitlePromoParameterName,
                    ParamName = "Promotional Title",
                    ParamHelp = "Title printed on vouchers for promotional cashable credits.",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(
                            AccountingConstants.TicketTitlePromo,
                            AccountingConstants.DefaultNonCashTicketTitle),
                        Constants.VoucherTitle16),
                    DefaultValue = AccountingConstants.DefaultNonCashTicketTitle
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleNonCashParameterName,
                    ParamName = "Non-Cashable Title",
                    ParamHelp = "Title printed on vouchers for non-cashable credits",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        string.IsNullOrEmpty(
                            _propertiesManager.GetValue(AccountingConstants.TicketTitleNonCash, string.Empty))
                            ? Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PlayableOnly)
                            : _propertiesManager.GetValue(AccountingConstants.TicketTitleNonCash, string.Empty),
                        Constants.VoucherTitle16),
                    DefaultValue = AccountingConstants.DefaultNonCashTicketTitle
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleLargeWinParameterName,
                    ParamName = "Large Win Title",
                    ParamHelp = "Title printed on vouchers for large wins (jackpots)",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(
                            AccountingConstants.TicketTitleLargeWin,
                            AccountingConstants.DefaultLargeWinTicketTitle),
                        Constants.VoucherTitle16),
                    DefaultValue = AccountingConstants.DefaultLargeWinTicketTitle
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleBonusCashParameterName,
                    ParamName = "Bonus Cashable Title",
                    ParamHelp = "Title printed on bonus award vouchers for cashable credits.",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(AccountingConstants.TicketTitleBonusCash, string.Empty),
                        Constants.VoucherTitle16),
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleBonusPromoParameterName,
                    ParamName = "Bonus Promotional Title",
                    ParamHelp = "Title printed on bonus award vouchers for promotional cashable credits",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(AccountingConstants.TicketTitleBonusPromo, string.Empty),
                        Constants.VoucherTitle16),
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleBonusNonCashParameterName,
                    ParamName = "Bonus Non-Cashable Title",
                    ParamHelp = "Title printed on bonus award vouchers for non-cashable credits",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(AccountingConstants.TicketTitleBonusNonCash, string.Empty),
                        Constants.VoucherTitle16),
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleWatCashParameterName,
                    ParamName = "WAT Cashable Title",
                    ParamHelp = "Title printed on WAT transfer vouchers for cashable credits",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(AccountingConstants.TicketTitleWatCash, string.Empty),
                        Constants.VoucherTitle16),
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleWatPromoParameterName,
                    ParamName = "WAT Promotional Title",
                    ParamHelp = "Title printed on WAT transfer vouchers for promotional cashable credits",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(AccountingConstants.TicketTitleWatPromo, string.Empty),
                        Constants.VoucherTitle16),
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.TitleWatNonCashParameterName,
                    ParamName = "WAT Non-Cashable Title",
                    ParamHelp = "Title printed on WAT transfer vouchers for non-cashable credits.",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    Value = Truncate(
                        _propertiesManager.GetValue(AccountingConstants.TicketTitleWatNonCash, string.Empty),
                        Constants.VoucherTitle16),
                    DefaultValue = string.Empty
                }
            };

            return BuildOptionItem(
                OptionConstants.VoucherTextFieldsOptionId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_textFields",
                "Voucher Text Fields List",
                "Text fields used by this voucher device",
                parameters,
                includeDetails);
        }

        private optionItem BuildVoucherOptions2(IVoucherDevice device, bool includeDetails)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.AllowVoucherIssueParameterName,
                    ParamName = "Allow Voucher Issuance",
                    ParamHelp = "Indicates whether validation data may be requested.",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = (bool)propertiesManager.GetProperty(AccountingConstants.VoucherOut, false),
                    DefaultValue = VoucherDevice.DefaultAllowVoucherIssue
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.AllowVoucherRedeemParameterName,
                    ParamName = "Allow Voucher Redemption",
                    ParamHelp = "Indicates whether vouchers may be redeemed.",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = (bool)propertiesManager.GetProperty(PropertyKey.VoucherIn, false),
                    DefaultValue = VoucherDevice.DefaultAllowVoucherRedeem
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.PrintNonCashOfflineParameterName,
                    ParamName = "Print Non-Cashable Voucher When Offline",
                    ParamHelp =
                        "Indicates whether vouchers for non-cashable credits may be printed while offline.",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.PrintNonCashOffLine,
                    DefaultValue = VoucherDevice.DefaultPrintNonCashOffLine
                }
            };

            return BuildOptionItem(
                "G2S_voucherOptions2",
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_voucherParams2",
                "Additional voucher Options",
                "Additional configuration parameters for this voucher device",
                parameters,
                includeDetails);
        }

        private optionItem BuildVoucherLimitsOption(IVoucherDevice device, bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.MaxOnLinePayOutParameterName,
                    ParamName = "Maximum Online Voucher",
                    ParamHelp = "Maximum value of an online voucher (in millicents)",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.MaxOnLinePayOut,
                    DefaultValue = VoucherDevice.DefaultMaxOnLinePayOut
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.MaxOffLinePayOutParameterName,
                    ParamName = "Maximum Offline Voucher",
                    ParamHelp = "Maximum value of an offline voucher (in millicents)",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.MaxOnLinePayOut,
                    DefaultValue = VoucherDevice.DefaultMaxOffLinePayOut
                }
            };

            return BuildOptionItem(
                "G2S_voucherLimits",
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_limitParams",
                "Voucher Limit Parameters",
                "Maximum values for printed vouchers",
                parameters,
                includeDetails);
        }

        private optionItem BuildVoucherOptions3(IVoucherDevice device, bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.CashOutToVoucherParameterName,
                    ParamName = "Cash Out to Voucher",
                    ParamHelp = "Indicates if device can be used for cashouts and other generic voucher issues",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.CashOutToVoucher,
                    DefaultValue = VoucherDevice.DefaultCashOutToVoucher
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.VoucherDevice.RedeemPrefixParameterName,
                    ParamName = "Redeem Prefix",
                    ParamHelp =
                        "Empty string or 2-digit prefix indicating validationId values supported for redemption",
                    ParamCreator = () => new stringParameter { canModRemote = true },
                    ValueCreator = () => new stringValue1(),
                    Value = (string)_propertiesManager.GetProperty(AccountingConstants.RedeemText, string.Empty),
                    DefaultValue = string.Empty
                }
            };

            return BuildOptionItem(
                "G2S_voucherOptions3",
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_voucherParams3",
                "Additional voucher Options",
                "Additional configuration parameters for this voucher device",
                parameters,
                includeDetails);
        }

        private string Truncate(string source, int length)
        {
            if (source.Length > length)
            {
                source = source.Substring(0, length);
            }

            return source;
        }
    }
}