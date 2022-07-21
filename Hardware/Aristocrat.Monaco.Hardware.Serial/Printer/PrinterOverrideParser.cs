namespace Aristocrat.Monaco.Hardware.Serial.Printer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Contracts.TicketContent;
    using log4net;
    using Serial;

    /// <summary>
    ///     This class handles loading/parsing ticket overrides from the jurisdiction
    ///     PrinterOverrides.xml file to be used in the Resolver and TemplateMappings classes
    /// </summary>
    public static class PrinterOverrideParser
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static List<OverridesOverride> _overrides;

        public static void LoadOverrides(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Logger.Debug("Invalid filename for Override file");
                return;
            }

            if (_overrides != null)
            {
                return;
            }

            // open file and deserialize xml
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var xmlSerializer = new XmlSerializer(typeof(Overrides));

                var contentsOfOverridesXml = (Overrides)xmlSerializer.Deserialize(fs);

                if (contentsOfOverridesXml == null)
                {
                    var message = fileName + " Deserialization failed.";

                    Logger.Error(message);
                    throw new InvalidTicketConfigurationException(message);
                }

                Logger.Debug($"Contents of override has {contentsOfOverridesXml.Override.Length} entries");

                _overrides = contentsOfOverridesXml.Override.ToList();
            }
        }

        public static IEnumerable<OverridesOverrideTemplateChange> GetTemplateOverrides(string protocol, string firmware)
        {
            Logger.Debug($"Trying to read template overrides. There are {_overrides.Count} entries");
            var result = GetPrinterSpecificOverride(protocol, firmware)?.
                TemplateChanges?.ToList();

            Logger.Debug(
                result == null
                    ? $"No TemplateChanges found for protocol '{protocol}' and firmware '{firmware}'"
                    : $"Got {result.Count} results. Result[0] is {result[0].Regions}");
            return result;
        }

        public static IEnumerable<OverridesOverrideMapping> GetMappingOverrides(string protocol, string firmware)
        {
            Logger.Debug($"Trying to read mapping overrides. There are {_overrides.Count} entries");
            var result = GetPrinterSpecificOverride(protocol, firmware)?.
                PrinterTemplateMappings?.ToList();

            Logger.Debug(
                result == null
                    ? $"No PrinterTemplateMappings found for protocol '{protocol}' and firmware '{firmware}'"
                    : $"Got {result.Count} results");
            return result;
        }

        public static OverridesOverride GetPrinterSpecificOverride(string protocol, string firmware)
        {
            Logger.Debug($"_overrides has '{_overrides?.Count}' entries. Protocol is '{protocol}'. Firmware is '{firmware}'");
            if (_overrides == null || string.IsNullOrEmpty(firmware) || string.IsNullOrEmpty(protocol))
            {
                Logger.Debug("null overrides, protocol, or firmware");
                return null;
            }

            Logger.Debug("_overrides and firmware is not null");

            foreach (var override1 in _overrides)
            {
                if (!protocol.Equals(override1.Protocol))
                {
                    Logger.Debug($"Skipping protocol '{override1.Protocol}'");
                    continue;
                }

                var firmwareChoices = override1.FirmwareBase.Split(
                    new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);
                foreach (var f in firmwareChoices)
                {
                    Logger.Debug($"Checking protocol '{override1.Protocol}' and firmware '{f}'");
                    if (f.Equals("*") || firmware.StartsWith(f))
                    {
                        Logger.Debug("Found a matching protocol/firmware");
                        return override1;
                    }
                }
            }

            Logger.Debug("No overrides found for this protocol/firmware combination");
            return null;
        }

        public static OverridesOverrideNewPrinterTemplates GetNewTemplates(string protocol, string firmware)
        {
            Logger.Debug($"Trying to read new template definitions. There are {_overrides.Count} entries");
            var result = GetPrinterSpecificOverride(protocol, firmware)?.NewPrinterTemplates;

            Logger.Debug(
                result == null
                    ? $"No NewPrinterTemplates found for protocol '{protocol}' and firmware '{firmware}'"
                    : $"Got '{result.NewRegion?.Length ?? 0}' new region results and '{result.NewTemplate?.Length ?? 0}' new templates");
            return result;
        }
    }
}