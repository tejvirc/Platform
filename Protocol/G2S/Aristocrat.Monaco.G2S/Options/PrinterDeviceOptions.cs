namespace Aristocrat.Monaco.G2S.Options
{
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.TicketContent;
    using Kernel;

    /// <inheritdoc />
    public class PrinterDeviceOptions : BaseDeviceOptions
    {
        private const string PrinterTemplateTableParameterName = "G2S_printerTemplate";
        private const string TemplateIndexParameterName = "G2S_templateIndex";
        private const string TemplateConfigParameterName = "G2S_templateConfig";

        private const string PrinterRegionTableParameterName = "G2S_printerRegion";
        private const string RegionIndexParameterName = "G2S_regionIndex";
        private const string RegionConfigParameterName = "G2S_regionConfig";

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Printer;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetPrinterTemplate(optionConfigValues);
            SetPrinterRegion(optionConfigValues);
        }

        private void SetPrinterTemplate(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(PrinterTemplateTableParameterName))
            {
                var table = optionConfigValues.GetTableValue(PrinterTemplateTableParameterName);

                var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();

                foreach (var tableRow in table)
                {
                    if (tableRow.HasValue(TemplateIndexParameterName) && tableRow.HasValue(TemplateConfigParameterName))
                    {
                        var templateConfig =
                            tableRow.GetDeviceOptionConfigValue(TemplateConfigParameterName).StringValue();

                        var serializer = new XmlSerializer(typeof(dpttype));
                        dpttype dptElement;

                        using (var reader = new StringReader(templateConfig))
                        {
                            dptElement = (dpttype)serializer.Deserialize(reader);
                        }

                        if (dptElement != null)
                        {
                            var regionStrings = dptElement.Value.Trim().Split(' ');
                            var numericalRegions =
                            (from str in regionStrings
                                where !string.IsNullOrEmpty(str)
                                select int.Parse(str, CultureInfo.InvariantCulture)).ToList();

                            var printableTemplate = new PrintableTemplate(
                                dptElement.name,
                                dptElement.id,
                                dptElement.t_dim_da,
                                dptElement.t_dim_pa,
                                numericalRegions);

                            printer.AddPrintableTemplate(printableTemplate);
                        }
                    }
                }
            }
        }

        private void SetPrinterRegion(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(PrinterRegionTableParameterName))
            {
                var table = optionConfigValues.GetTableValue(PrinterRegionTableParameterName);

                var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();

                foreach (var tableRow in table)
                {
                    if (tableRow.HasValue(RegionIndexParameterName) && tableRow.HasValue(RegionConfigParameterName))
                    {
                        var regionConfig =
                            tableRow.GetDeviceOptionConfigValue(RegionConfigParameterName).StringValue();

                        var serializer = new XmlSerializer(typeof(dprtype));
                        dprtype dprElement;

                        using (TextReader reader = new StringReader(regionConfig))
                        {
                            dprElement = (dprtype)serializer.Deserialize(reader);
                        }

                        if (dprElement != null)
                        {
                            var id = int.Parse(dprElement.id, CultureInfo.InvariantCulture);
                            var rotation = int.Parse(dprElement.rot, CultureInfo.InvariantCulture);
                            var defaultText = dprElement.D;

                            var printableRegion = new PrintableRegion(
                                dprElement.name,
                                dprElement.property,
                                id,
                                dprElement.x,
                                dprElement.px,
                                dprElement.y,
                                dprElement.py,
                                dprElement.dx,
                                dprElement.pdx,
                                dprElement.dy,
                                dprElement.pdy,
                                rotation,
                                dprElement.jst,
                                dprElement.type,
                                dprElement.m1,
                                dprElement.m2,
                                dprElement.attr,
                                defaultText);

                            printer.AddPrintableRegion(printableRegion);
                        }
                    }
                }
            }
        }
    }
}