namespace Aristocrat.Monaco.Hardware.Serial.Printer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;
    using Contracts.TicketContent;
    using log4net;

    /// <summary>
    ///     Utility methods to map printable templates and regions to printer defined templates.
    /// </summary>
    public static class TemplateMapping
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Get mapping nodes for a specific printer protocol/firmware.
        ///     This will return null if the protocol/firmware is not mapped.
        /// </summary>
        /// <param name="mappings">The printer template mappings.</param>
        /// <param name="protocol">The printer protocol.</param>
        /// <param name="firmware">The printer firmware.</param>
        /// <returns>The PrinterTemplateMappingsPrinterMapping object.</returns>
        public static IEnumerable<OverridesOverrideMapping> GetPrinterSpecificMappings(
            this Overrides mappings,
            string protocol,
            string firmware)
        {
            var map = mappings?.Override?.FirstOrDefault(x => x.Protocol == protocol && firmware.StartsWith(x.FirmwareBase))?.PrinterTemplateMappings.ToList() ??
                      mappings?.Override?.FirstOrDefault(x => x.Protocol == protocol && x.FirmwareBase == "*")?.PrinterTemplateMappings.ToList();

            return map;
        }

        /// <summary>
        ///     Determines whether or not a protocol/firmware pair has a mapping configuration.
        /// </summary>
        /// <param name="mappings">The printer template mappings.</param>
        /// <param name="protocol">The printer protocol.</param>
        /// <param name="firmware">The printer firmware.</param>
        /// <returns>Returns true when the protocol/firmware is configured.  Otherwise, returns false.</returns>
        public static bool IsProtocolConfigured(this Overrides mappings, string protocol, string firmware)
        {
            return mappings.GetPrinterSpecificMappings(protocol, firmware) != null;
        }

        /// <summary>
        ///     Determines whether or not a PrintCommand is for an audit ticket.
        /// </summary>
        /// <param name="mappings">The printer template mappings.</param>
        /// <param name="command">The print command.</param>
        /// <returns></returns>
        public static bool IsAuditTicket(this OverridesOverride mappings, PrintCommand command)
        {
            return mappings.AuditTickets?.PlatformTemplateIds?.Contains(command.Id.ToString()) ?? false;
        }

        /// <summary>
        ///     Load the mappings configuration file into an object.
        /// </summary>
        /// <param name="fileName">The path to the config file.</param>
        /// <returns>The PrinterTemplateMappings object.</returns>
        public static Overrides LoadMappings(string fileName)
        {
            return LoadConfigFile<Overrides>(fileName);
        }

        /// <summary>
        ///     Remap a print command's data to use a printer built in template
        /// </summary>
        /// <param name="mappings">The printer protocol specific mappings.</param>
        /// <param name="printCommand">The print command.</param>
        /// <param name="printableTemplates">The platform printable template definitions.</param>
        /// <param name="printableRegions">The platform printable region definitions.</param>
        /// <returns>the remapped print command</returns>
        public static PrintCommand RemapPrintCommand(
            this OverridesOverride mappings,
            PrintCommand printCommand,
            IEnumerable<dpttype> printableTemplates,
            IEnumerable<dprtype> printableRegions)
        {
            var printableTemplate = GetPlatformTemplate(printCommand.Id, printableTemplates);
            if (printableTemplate is null)
            {
                return printCommand;
            }

            Logger.Debug($"Platform Template Id {printCommand.Id} regions are {printableTemplate.Value}");

            var templateMappings = GetTemplateMappings(mappings, printCommand.Id.ToString());
            if (templateMappings is null)
            {
                // nothing to remap so just give back the original information
                return printCommand;
            }

            var regionsInTemplate = printableTemplate.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // check if any Replacement elements apply to any of the regions
            var newPrintableRegions =
                HandleReplacements(mappings, regionsInTemplate, printCommand, printableRegions);

            var remappedPrintData = templateMappings.Regions.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(
                    region => region == "0"
                        ? string.Empty
                        : GetRegionPrintData(region, newPrintableRegions))
                .ToList();

            var mappedCommand = new PrintCommand
            {
                UsePrinterTemplate = true,
                PrinterTemplateId = templateMappings.PrinterTemplateId,
                DataFields = new PrintDataField[remappedPrintData.Count]
            };

            for (var i = 0; i < remappedPrintData.Count; i++)
            {
                mappedCommand.DataFields[i] = new PrintDataField { Data = remappedPrintData[i] };
            }

            return mappedCommand;
        }

        public static IEnumerable<dprtype> HandleReplacements(OverridesOverride mapping, string[] regionsInTemplate, PrintCommand printCommand, IEnumerable<dprtype> printableRegions)
        {
            if (mapping.Replacements is null)
            {
                return printableRegions;
            }

            var newPrintableRegions = new List<dprtype>();

            // do all the text substitutions first
            foreach (var regionId in regionsInTemplate)
            {
                // get the data for this region
                var regionText = GetTextForRegionId(regionId);
                if (regionText is null)
                {
                    return printableRegions;
                }

                Logger.Debug($"regionId {regionId} has text {regionText}");

                // check for global replacements
                regionText = GlobalReplaceRegionText(regionText);

                // check for region specific replacements
                regionText = ReplaceRegionText(regionId, regionText);

                // check for region case change
                regionText = ChangeRegionTextCase(regionId, regionText);
                UpdateTextForRegionId(regionId, regionText);
            }

            // now do the text split/joins
            foreach (var regionId in regionsInTemplate)
            {
                // get the data for this region
                var regionText = GetTextForRegionId(regionId);
                if (regionText is null)
                {
                    return printableRegions;
                }

                Logger.Debug($"regionId {regionId} has text {regionText}");

                // check for text splitting one region into 2 new regions
                var skipAdd = SplitRegionText(regionId, regionText, false);

                // check for combining 2 regions into 1 new one
                skipAdd = CombineRegionText(regionId, regionText, skipAdd);

                if (!skipAdd)
                {
                    Logger.Debug($"adding regionId {regionId} with text {regionText}");
                    newPrintableRegions.Add(new dprtype { id = regionId, property = regionText });
                }
            }

            return newPrintableRegions;

            //////////////// Local Methods //////////////
            string GlobalReplaceRegionText(string regionText)
            {
                if (mapping.Replacements.GlobalRegionTextReplace is null)
                {
                    return regionText;
                }

                foreach (var replacement in mapping.Replacements.GlobalRegionTextReplace)
                {
                    var regex = new Regex(Regex.Escape(replacement.Replace));
                    regionText = replacement.OnlyFirstOccurrencePerLine
                        ? regex.Replace(regionText, replacement.With, 1)
                        : regex.Replace(regionText, replacement.With);
                }

                return regionText;
            }

            string ChangeRegionTextCase(string regionId, string regionText)
            {
                if (mapping.Replacements.ChangeRegionCase is null)
                {
                    return regionText;
                }

                return mapping.Replacements.ChangeRegionCase
                    .Where(changeCase => regionId == changeCase.RegionId)
                    .Aggregate(regionText, (current, changeCase) => current.ToUpper());
            }

            string ReplaceRegionText(string regionId, string regionText)
            {
                if (mapping.Replacements.RegionTextReplace is null)
                {
                    return regionText;
                }

                foreach (var replacement in mapping.Replacements.RegionTextReplace)
                {
                    // check if we have a PlatformTemplateId and it matches the current id
                    if (!string.IsNullOrEmpty(replacement.PlatformTemplateIds))
                    {
                        var ids = replacement.PlatformTemplateIds.Split(
                            new[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (!ids.Contains(printCommand.Id.ToString()))
                        {
                            continue;
                        }
                    }

                    // if we don't have a PlatformTemplateId then the replace applies to all regions
                    if (regionId == replacement.RegionId)
                    {
                        var regex = new Regex(replacement.IsReplaceRegexString ? replacement.Replace : Regex.Escape(replacement.Replace));
                        regionText = replacement.LeadingText + regex.Replace(regionText, replacement.With, 1);
                    }
                }

                return regionText;
            }

            string GetTextForRegionId(string regionId)
            {
                var index = Array.IndexOf(regionsInTemplate, regionId);
                if (index == -1)
                {
                    // this regionId isn't in the template so just return the original command
                    return null;
                }

                return printCommand.DataFields[index].Data ?? string.Empty;
            }

            void UpdateTextForRegionId(string regionId, string regionText)
            {
                var index = Array.IndexOf(regionsInTemplate, regionId);
                if (index == -1)
                {
                    // this regionId isn't in the template so just return the original command
                    return;
                }

                printCommand.DataFields[index].Data = regionText;
            }

            bool SplitRegionText(string regionId, string regionText, bool skipAdd)
            {
                if (mapping.Replacements.TextSplitForRegion is null)
                {
                    return skipAdd;
                }

                foreach (var replacement in mapping.Replacements.TextSplitForRegion)
                {
                    if (replacement.RegionId != regionId)
                    {
                        continue;
                    }

                    var splitText = string.Empty;
                    if (!string.IsNullOrEmpty(replacement.RegEx))
                    {
                        var regex = new Regex(replacement.RegEx, RegexOptions.IgnoreCase);
                        var match = regex.Match(regionText);

                        if (match.Groups.Count > 1)
                        {
                            splitText = replacement.LeadingText + match.Groups[1];
                        }

                        Logger.Debug($"replacing regionId {regionId} and text {regionText} with newId {replacement.NewRegionId} and text {splitText}");
                        newPrintableRegions.Add(new dprtype { id = replacement.NewRegionId, property = splitText });
                        skipAdd = true;
                    }
                    else if (!string.IsNullOrEmpty(replacement.FormatString))
                    {
                        // try to format date time string
                        if (DateTime.TryParse(regionText, out var result))
                        {
                            splitText = result.ToString(replacement.FormatString);
                            Logger.Debug(
                                $"formatting datetime regionId {regionId} and text {regionText} with newId {replacement.NewRegionId} and text {splitText}");
                            newPrintableRegions.Add(new dprtype { id = replacement.NewRegionId, property = splitText });
                            skipAdd = true;
                        }
                    }
                }

                return skipAdd;
            }

            bool CombineRegionText(string regionId, string regionText, bool skipAdd)
            {
                if (mapping.Replacements.TextCombine is null)
                {
                    return skipAdd;
                }

                foreach (var combine in mapping.Replacements.TextCombine)
                {
                    // check if text join should occur on the current ticket type
                    var ids = combine.PlatformTemplateIds.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (!ids.Contains(printCommand.Id.ToString()))
                    {
                        continue;
                    }

                    var regionText2 = string.Empty;
                    var combineRegions = combine.RegionIds.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // get text for both regions only if the first region is the current one
                    if (combineRegions[0] != regionId)
                    {
                        continue;
                    }

                    // are there 2 regions
                    if (combineRegions.Length > 1)
                    {
                        regionText2 = GetTextForRegionId(combineRegions[1]);
                        if (regionText2 is null)
                        {
                            // this regionId isn't in the template so just return the original command
                            {
                                return true;
                            }
                        }
                    }

                    // join all the text
                    regionText2 = string.Join(
                        "",
                        combine.LeadingText,
                        regionText,
                        combine.JoinText,
                        regionText2,
                        combine.TrailingText);

                    // replace special characters
                    regionText2 = regionText2.Replace(@"\r", "~013")
                        .Replace(@"\n", "~010");

                    Logger.Debug(
                        $"replacing regionId {regionId} and text {regionText} with newId {combine.NewRegionId} and text {regionText2}");
                    newPrintableRegions.Add(new dprtype { id = combine.NewRegionId, property = regionText2 });
                    skipAdd = true;
                }

                return skipAdd;
            }
        }

        /// <summary>
        ///     Gets the desired font based on the original font number.
        /// </summary>
        /// <param name="mappings">The printer protocol specific mappings.</param>
        /// <param name="originalFontNumber">The font used to map to the desired font</param>
        /// <param name="region">The region used to get the specific mapping</param>
        /// <returns></returns>
        public static string GetSpecificFont(this OverridesOverride mappings, string originalFontNumber, string region)
        {
            if (mappings is null || mappings.FontOverrides is null)
            {
                return string.Empty;
            }

            foreach (var f in mappings.FontOverrides)
            {
                if (f.RegionIds.Contains(region) && f.OriginalFontNumber == originalFontNumber)
                {
                    return f.NewFontNumber;
                }
            }

            return string.Empty;
        }

        private static string GetRegionPrintData(
            string region,
            IEnumerable<dprtype> printableRegions)
        {
            return printableRegions.FirstOrDefault(x => x.id == region)?.property ?? string.Empty;
        }

        private static OverridesOverrideMapping GetTemplateMappings(
            OverridesOverride mappings,
            string platformTemplateId)
        {
            return mappings.PrinterTemplateMappings
                .FirstOrDefault(x => x.PlatformTemplateId == platformTemplateId);
        }

        private static T LoadConfigFile<T>(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                var xmlContent = (T)xmlSerializer.Deserialize(fileStream);

                if (xmlContent != null)
                {
                    return xmlContent;
                }

                var message = fileName + " Deserialization failed.";
                Logger.Error(message);
                throw new InvalidTicketConfigurationException(message);
            }
        }

        private static dpttype GetPlatformTemplate(int id, IEnumerable<dpttype> printableTemplates)
        {
            return printableTemplates.FirstOrDefault(x => x.id == id);
        }
    }
}