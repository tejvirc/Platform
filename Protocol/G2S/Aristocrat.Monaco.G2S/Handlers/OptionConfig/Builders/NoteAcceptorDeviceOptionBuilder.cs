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
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts;
    using NoteAcceptor;

    /// <inheritdoc />
    public class NoteAcceptorDeviceOptionBuilder : BaseDeviceOptionBuilder<NoteAcceptorDevice>
    {
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorDeviceOptionBuilder" /> class.
        /// </summary>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        public NoteAcceptorDeviceOptionBuilder(IDeviceRegistryService deviceRegistry, IPropertiesManager properties)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.NoteAcceptor;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            NoteAcceptorDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            // chapter 13.21.1
            var group = new optionGroup
            {
                optionGroupId = "G2S_noteAcceptorOptions",
                optionGroupName = "G2S Note Acceptor Options"
            };

            var items = new List<optionItem>();

            // chapter 13.21.2
            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for the note acceptor device",
                        parameters.IncludeDetails));
            }

            // chapter 13.21.3
            if (ShouldIncludeParam(OptionConstants.NoteAcceptorOptionsId, parameters))
            {
                items.Add(BuildNoteAcceptorOptions(parameters.IncludeDetails));
            }

            // chapter 13.21.4
            if (ShouldIncludeParam(OptionConstants.NoteAcceptorDataTable, parameters))
            {
                items.Add(BuildNoteAcceptorDataTable(parameters.IncludeDetails));
            }

            // chapter 13.21.5
            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildNoteAcceptorOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId =
                        G2SParametersNames.NoteAcceptor
                            .NoteEnabledParameterName,
                    ParamName = "Notes Enabled",
                    ParamHelp = "Note acceptor should accept notes",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = ServiceManager.GetInstance().TryGetService<INoteAcceptor>()?.Denominations.Any() ?? false
                },
                new ParameterDescription
                {
                    ParamId =
                        G2SParametersNames.NoteAcceptor
                            .VoucherEnabledParameterName,
                    ParamName = "Vouchers Enabled",
                    ParamHelp = "Note acceptor should accept vouchers",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = _properties.GetValue(PropertyKey.VoucherIn, false)
                }
            };

            return BuildOptionItem(
                OptionConstants.NoteAcceptorOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_noteAcceptorParams",
                "Note Acceptor Options",
                "Options that control note acceptor device functions",
                parameters,
                includeDetails);
        }
        
        private optionItem BuildNoteAcceptorDataTable(bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CurrencyIdParameterName,
                    ParamName = "Currency ID",
                    ParamHelp = "Currency identifier",
                    ParamCreator = () => new stringParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.NoteAcceptor.DenomIdParameterName,
                    ParamName = "Denomination ID",
                    ParamHelp = "Denomination identifier",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.NoteAcceptor.BaseCashableAmountParameterName,
                    ParamName = "Exchange Value Per Unit",
                    ParamHelp = "Actual value (in millicents)",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.NoteAcceptor.NoteActiveParameterName,
                    ParamName = "Note Activated",
                    ParamHelp = "Note can be accepted",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.NoteAcceptor.TokenParameterName,
                    ParamName = "Note is a token",
                    ParamHelp = "This note is a coupon",
                    ParamCreator = () => new booleanParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.NoteAcceptor.BasePromoAmountParameterName,
                    ParamName = "Promotional Exchange Value Per Unit",
                    ParamHelp = "Actual promotional value of this note (in millicents)",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.NoteAcceptor.BaseNonCashAmountParameterName,
                    ParamName = "Non-Cashable Exchange Value Per Unit",
                    ParamHelp = "Actual non-cashable value of this note (in millicents)",
                    ParamCreator = () => new integerParameter()
                }
            };

            var noteAcceptor = _deviceRegistry.GetDevice<INoteAcceptor>();

            var currencyId = _properties.GetValue(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId);

            var currentValues = noteAcceptor?.GetNoteAcceptorData(currencyId).Select(
                n => new complexValue
                {
                    paramId = G2SParametersNames.NoteAcceptor.NoteAcceptorDataParameterName,
                    Items = new object[]
                    {
                        new stringValue1
                        {
                            paramId = G2SParametersNames.CurrencyIdParameterName,
                            Value = n.currencyId
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.DenomIdParameterName,
                            Value = n.denomId
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.BaseCashableAmountParameterName,
                            Value = n.baseCashableAmt
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.NoteActiveParameterName,
                            Value = n.noteActive
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.TokenParameterName,
                            Value = n.token
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.BasePromoAmountParameterName,
                            Value = n.basePromoAmt
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.BaseNonCashAmountParameterName,
                            Value = n.baseNonCashAmt
                        }
                    }
                });

            var defaultValues = noteAcceptor?.GetNoteAcceptorData(currencyId).Select(
                n => new complexValue
                {
                    paramId = G2SParametersNames.NoteAcceptor.NoteAcceptorDataParameterName,
                    Items = new object[]
                    {
                        new stringValue1
                        {
                            paramId = G2SParametersNames.CurrencyIdParameterName,
                            Value = n.currencyId
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.DenomIdParameterName,
                            Value = n.denomId
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.BaseCashableAmountParameterName,
                            Value = n.baseCashableAmt
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.NoteActiveParameterName,
                            Value = true
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.TokenParameterName,
                            Value = n.token
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.BasePromoAmountParameterName,
                            Value = n.basePromoAmt
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.NoteAcceptor.BaseNonCashAmountParameterName,
                            Value = n.baseNonCashAmt
                        }
                    }
                });

            return BuildOptionItemForTable(
                OptionConstants.NoteAcceptorDataTable,
                t_securityLevels.G2S_operator,
                defaultValues.Count(),
                defaultValues.Count(),
                G2SParametersNames.NoteAcceptor.NoteAcceptorDataParameterName,
                "Note Data",
                "Information on one note that can be accepted by the device",
                parameters,
                currentValues,
                defaultValues,
                includeDetails);
        }
    }
}
