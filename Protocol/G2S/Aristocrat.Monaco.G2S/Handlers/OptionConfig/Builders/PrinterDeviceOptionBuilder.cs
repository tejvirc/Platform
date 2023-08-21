namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Hardware.Contracts;
    using Hardware.Contracts.Printer;
    using Linq;

    /// <inheritdoc />
    public class PrinterDeviceOptionBuilder : BaseDeviceOptionBuilder<PrinterDevice>
    {
        private readonly IDeviceRegistryService _deviceRegistryService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterDeviceOptionBuilder" /> class.
        /// </summary>
        /// <param name="deviceRegistryService">An <see cref="IDeviceRegistryService" /> instance.</param>
        public PrinterDeviceOptionBuilder(IDeviceRegistryService deviceRegistryService)
        {
            _deviceRegistryService =
                deviceRegistryService ?? throw new ArgumentNullException(nameof(deviceRegistryService));
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.Printer;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            PrinterDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_printerOptions",
                optionGroupName = "G2S Printer Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for the printer device",
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.PrinterTemplateTableOptionsId, parameters))
            {
                items.Add(BuildPrinterTemplateTableOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.PrinterRegionTableOptionsId, parameters))
            {
                items.Add(BuildPrinterRegionTableOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.AdditionalPrinterOptionsId, parameters))
            {
                items.Add(BuildAdditionalPrinterOptions(parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            var igtGroup = new optionGroup
            {
                optionGroupId = "IGT_printerOptions",
                optionGroupName = "IGT Printer Options"
            };

            if (ShouldIncludeParam(OptionConstants.IgtPrinterOptionsId, parameters))
            {
                igtGroup.optionItem = new[] { BuildIgtPrinterOptions(parameters.IncludeDetails) };
            }

            return new[] { group, igtGroup };
        }

        private optionItem BuildPrinterTemplateTableOptions(bool includeDetails)
        {
            var printer = _deviceRegistryService.GetDevice<IPrinter>();

            var printerTemplatesValues = printer.Templates.Select(
                template =>
                    new complexValue
                    {
                        paramId = "G2S_printerTemplate",
                        Items = new object[]
                        {
                            new integerValue1
                            {
                                paramId = G2SParametersNames.PrinterDevice.TemplateIndexParameterName,
                                Value = template.Id
                            },
                            new stringValue1
                            {
                                paramId = G2SParametersNames.PrinterDevice.TemplateConfigParameterName,
                                Value = template.ToPDL()
                            }
                        }
                    });

            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PrinterDevice.TemplateIndexParameterName,
                    ParamName = "Template Index",
                    ParamHelp = "Printer template index",
                    ParamCreator = () => new integerParameter { minIncl = 0, maxIncl = 999 }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PrinterDevice.TemplateConfigParameterName,
                    ParamName = "Template Configuration",
                    ParamHelp = "Template configuration string",
                    ParamCreator = () => new stringParameter()
                }
            };

            // TODO: Get base templates vs. the configured list
            return BuildOptionItemForTable(
                OptionConstants.PrinterTemplateTableOptionsId,
                t_securityLevels.G2S_operator,
                0,
                1,
                "G2S_printerTemplate",
                "Printer Template",
                "Printer template configuration options",
                parameters,
                printerTemplatesValues,
                printerTemplatesValues,
                includeDetails);
        }

        private optionItem BuildPrinterRegionTableOptions(bool includeDetails)
        {
            var printer = _deviceRegistryService.GetDevice<IPrinter>();

            var printerRegionsValues = printer.Regions.Select(
                region =>
                    new complexValue
                    {
                        paramId = "G2S_printerRegion",
                        Items = new object[]
                        {
                            new integerValue1
                            {
                                paramId = G2SParametersNames.PrinterDevice.RegionIndexParameterName,
                                Value = region.Id
                            },
                            new stringValue1
                            {
                                paramId = G2SParametersNames.PrinterDevice.RegionConfigParameterName,
                                Value = region.ToPDL(printer.UseLargeFont)
                            }
                        }
                    });

            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = "G2S_regionIndex",
                    ParamName = "Region Index",
                    ParamHelp = "Printer region index",
                    ParamCreator = () => new integerParameter { minIncl = 0, maxIncl = 999 }
                },
                new ParameterDescription
                {
                    ParamId = "G2S_regionConfig",
                    ParamName = "Region Configuration",
                    ParamHelp = "Region configuration string",
                    ParamCreator = () => new stringParameter()
                }
            };

            // TODO: Get base regions vs. the configured list
            return BuildOptionItemForTable(
                OptionConstants.PrinterRegionTableOptionsId,
                t_securityLevels.G2S_operator,
                printerRegionsValues.Count(),
                printerRegionsValues.Count(),
                "G2S_printerRegion",
                "Printer Region Configuration",
                "Printer region configuration options",
                parameters,
                printerRegionsValues,
                printerRegionsValues,
                includeDetails);
        }

        private optionItem BuildAdditionalPrinterOptions(bool includeDetails)
        {
            var parameters = new List<ParameterDescription> // TODO
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PrinterDevice.HostInitiatedParameterName,
                    ParamName = "Host-Initiated Print Requests",
                    ParamHelp = "Indicates whether host-initiated print requests are permitted.",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = t_g2sBoolean.G2S_false.ToName(),
                    DefaultValue = t_g2sBoolean.G2S_false.ToName()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PrinterDevice.CustomTemplatesParameterName,
                    ParamName = "Custom Templates",
                    ParamHelp = "Indicates whether site-specific templates can be configured.",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = t_g2sBoolean.G2S_false.ToName(),
                    DefaultValue = t_g2sBoolean.G2S_false.ToName()
                }
            };

            return BuildOptionItem(
                OptionConstants.AdditionalPrinterOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_additionalPrinterParams",
                "Additional Printer Parameters",
                "Additional parameters for printers.",
                parameters,
                includeDetails);
        }

        private optionItem BuildIgtPrinterOptions(bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription // TODO
                {
                    ParamId = G2SParametersNames.PrinterDevice.IgtIdReaderIdParameterName,
                    ParamName = "ID Reader to Use",
                    ParamHelp = "ID reader to reference when printing tickets " +
                                "(0 = none, -1 = any ID reader of the specified ID reader type).",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    Value = 0,
                    DefaultValue = 0
                }
            };

            return BuildOptionItem(
                OptionConstants.IgtPrinterOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "IGT_printerOptions",
                "IGT Printer Options",
                "Additional Printer Options",
                parameters,
                includeDetails);
        }
    }
}