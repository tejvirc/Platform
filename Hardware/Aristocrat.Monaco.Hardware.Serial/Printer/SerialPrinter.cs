namespace Aristocrat.Monaco.Hardware.Serial.Printer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Contracts.Gds;
    using Contracts.Gds.Printer;
    using Kernel;
    using log4net;
    using Protocols;

    /// <summary>A serial printer.</summary>
    public abstract class SerialPrinter : SerialDeviceProtocol
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const string PrinterOverridesExtensionPath = "/Hardware/PrinterOverrides";

        private PrinterStatus _printerStatus = new PrinterStatus { TopOfForm = true };
        private TicketPrintStatus _ticketPrintStatus = new TicketPrintStatus();
        private List<OverridesOverrideTemplateChange> _templateOverrides;
        private readonly object _lock = new object();

        /// <summary>
        ///     Required Printer-class fonts
        ///     (taken from Section 7.9.4 of the v1.3 GDS Printer Spec)
        /// </summary>
        protected static readonly List<FontDefinition> PrFonts = new List<FontDefinition>
        {
            new FontDefinition{ Index = 1, HeightPts = 29.5f, PitchCpi = 2.5f },
            new FontDefinition{ Index = 2, HeightPts = 25.8f, PitchCpi = 3.3f },
            new FontDefinition{ Index = 3, HeightPts = 13.8f, PitchCpi = 5.5f },
            new FontDefinition{ Index = 4, HeightPts = 21.3f, PitchCpi = 4.0f },
            new FontDefinition{ Index = 5, HeightPts = 10.6f, PitchCpi = 7.3f },
            new FontDefinition{ Index = 6, HeightPts = 8.5f, PitchCpi = 20.3f },
            new FontDefinition{ Index = 7, HeightPts = 9.2f, PitchCpi = 10.1f },
            new FontDefinition{ Index = 8, HeightPts = 12.7f, PitchCpi = 5.7f },
        };

        /// <summary>
        /// Gets the fonts scaled based on scale factors.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fontScaleFactors"></param>
        /// <returns></returns>
        protected FontDefinition GetDeviceScaledFont(int index, IEnumerable<FontScaleFactor> fontScaleFactors)
        {
            var scaleFactor = fontScaleFactors.FirstOrDefault(f => f.Index == index)?.ScaleFactor ?? 1f;
            var font = PrFonts.FirstOrDefault(f => f.Index == index) ?? PrFonts[0];

            return new FontDefinition
            {
                Index = font.Index,
                HeightPts = font.HeightPts * scaleFactor,
                PitchCpi = font.PitchCpi * scaleFactor
            };
        }

        /// <summary>
        ///     Platform/printer template mappings for the selected printer.
        /// </summary>
        protected OverridesOverride PrinterSpecificTemplateMappings { get; set; }

        /// <summary>
        ///     Cache of printable regions.
        /// </summary>
        protected List<dprtype> RegionCache { get; } = new List<dprtype>();

        /// <summary>
        ///     Cache of printable templates.
        /// </summary>
        protected List<dpttype> TemplateCache { get; } = new List<dpttype>();

        /// <summary>
        /// true if we have overrides for printer template mappings
        /// </summary>
        protected bool UsePrinterDefinedTemplates => PrinterSpecificTemplateMappings?.PrinterTemplateMappings != null && PrinterSpecificTemplateMappings.UsePrinterDefinedTemplates;

        /// <summary>
        /// Get or set the PrinterStatus
        /// </summary>
        protected PrinterStatus PrinterStatus
        {
            get => _printerStatus;
            set
            {
                if (_printerStatus.Equals(value))
                {
                    return;
                }

                if (OnMessageReceived(value))
                {
                    _printerStatus = value;
                }
            }
        }

        /// <summary>
        /// Get or set the TicketPrintStatus
        /// </summary>
        protected TicketPrintStatus TicketPrintStatus
        {
            get => _ticketPrintStatus;
            set
            {
                if (_ticketPrintStatus.Equals(value))
                {
                    return;
                }

                if (OnMessageReceived(value))
                {
                    _ticketPrintStatus = value;
                }
            }
        }

        /// <inheritdoc/>
        protected override bool HasDisablingFault =>
            PrinterStatus.ChassisOpen || PrinterStatus.PaperJam || PrinterStatus.PrintHeadOpen;

        /// <inheritdoc/>
        protected override async void ProcessMessage(GdsSerializableMessage message, CancellationToken token)
        {
            switch (message.ReportId)
            {
                // For GDS printers
                case GdsConstants.ReportId.PrinterDefineRegion:
                    {
                        if (message is DefineRegion region)
                        {
                            var reader = new StringReader(region.Data);
                            var serializer = new XmlSerializer(typeof(dprtype));
                            var printableRegion = (dprtype)serializer.Deserialize(reader);
                            var result = await DefineRegion(printableRegion);
                            OnMessageReceived(new TransferStatus
                            {
                                StatusCode = GdsConstants.Success,
                                RegionCode = result
                            });
                        }
                        break;
                    }
                case GdsConstants.ReportId.PrinterDefineTemplate:
                    {
                        if (message is DefineTemplate template)
                        {
                            var reader = new StringReader(template.Data);
                            var serializer = new XmlSerializer(typeof(dpttype));
                            var printableTemplate = (dpttype)serializer.Deserialize(reader);
                            var result = await DefineTemplate(printableTemplate);
                            OnMessageReceived(new TransferStatus
                            {
                                StatusCode = GdsConstants.Success,
                                TemplateCode = result
                            });
                        }
                        break;
                    }
                case GdsConstants.ReportId.PrinterPrintTicket:
                    {
                        if (HasDisablingFault || !PrinterStatus.TopOfForm || !IsEnabled)
                        {
                            Logger.Debug("Can't print ticket because of status");
                            OnMessageReceived(PrinterStatus);
                        }
                        else if (message is PrintTicket ticket)
                        {
                            var reader = new StringReader(CleanTicket(ticket.Data));
                            var serializer = new XmlSerializer(typeof(PrintCommand));
                            var job = (PrintCommand)serializer.Deserialize(reader);
                            await PrintTicket(job, token);
                        }
                        break;
                    }
                case GdsConstants.ReportId.PrinterFormFeed:
                    if (HasDisablingFault)
                    {
                        OnMessageReceived(PrinterStatus);
                    }
                    await FormFeed();
                    break;
                case GdsConstants.ReportId.PrinterRequestMetrics:
                    OnMessageReceived(new Metrics { Data = string.Empty });
                    break;
                case GdsConstants.ReportId.PrinterGraphicTransferSetup:
                case GdsConstants.ReportId.PrinterFileTransfer:
                    OnMessageReceived(new TransferStatus
                    {
                        StatusCode = GdsConstants.Success,
                        GraphicCode = true
                    });
                    break;
                case GdsConstants.ReportId.PrinterTicketRetract:
                    PrinterStatus = new PrinterStatus
                    {
                        TopOfForm = true
                    };
                    break;

                // Other
                default:
                    base.ProcessMessage(message, token);
                    break;
            }
        }

        /// <summary>Tell printer to form feed.</summary>
        /// <returns>Success</returns>
        protected abstract Task<bool> FormFeed();

        /// <summary>Tell printer a region definition.</summary>
        /// <param name="region">Region definition</param>
        /// <returns>Success</returns>
        protected abstract Task<bool> DefineRegion(dprtype region);

        /// <summary>Tell printer a template definition (collection of regions).</summary>
        /// <param name="template">Template definition</param>
        /// <returns>Success</returns>
        protected virtual Task<bool> DefineTemplate(dpttype template)
        {
            lock (_lock)
            {
                Logger.Debug($"checking if template '{template.id}' {template.Value} should be replaced.");
                if (_templateOverrides != null && _templateOverrides.Count > 0)
                {
                    Logger.Debug($"There are {_templateOverrides.Count} template overrides");
                    var t = _templateOverrides.FirstOrDefault(v => v.PlatformTemplateId == template.id);
                    if (t != null)
                    {
                        Logger.Debug($"replacing template {template.id} with {t.Regions}");
                        TemplateCache.Add(
                            new dpttype { id = t.PlatformTemplateId, name = t.Name, Value = t.Regions }
                        );
                    }
                    else
                    {
                        Logger.Debug($"adding original template {template.id} with {template.Value}");
                        TemplateCache.Add(template);
                    }
                }
                else
                {
                    TemplateCache.Add(template);
                }
            }

            return Task.FromResult(UsePrinterDefinedTemplates || RenderTemplateDefinition(template));
        }

        protected abstract bool RenderRegionDefinition(dprtype pdlRegion);

        protected abstract bool RenderTemplateDefinition(dpttype pdlTemplate);

        /// <summary>Tell printer to print a ticket.</summary>
        /// <param name="ticket">Ticket description</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Success</returns>
        protected abstract Task<bool> PrintTicket(PrintCommand ticket, CancellationToken token);

        /// <summary>
        ///     Some printers have special characters that need to be swapped with a different characters
        ///     in order for them to print, i.e., the '₱' symbol must be swapped with the '¹' symbol. These
        ///     are called out in firmware documents and must be handled in the mapping or here
        /// </summary>
        /// <param name="line">The line data.</param>
        /// <returns>The pre-rendered region data.</returns>
        protected abstract void SpecialCaseCharacterReplacement(ref string line);

        /// <summary>
        /// Some characters are not supported by printer in line printing and print some garbage value on ticket,
        /// so we need to replace these characters with empty space
        /// </summary>
        /// <param name="line">The region ids data.</param>
        /// <returns>The pre-rendered region data.</returns>
        protected abstract void NotSupportedCharacterReplacement(ref string line);

        /// <summary>
        ///     Cleans special characters from a ticket so that it can be deserialized as XML
        /// </summary>
        /// <remarks>
        ///     The ticket is built manually rather than by a XML serializer.  This process allows
        ///     special characters that mess up XML to be in the document.  The ticket must be
        ///     cleaned of special characters before being deserialized.
        /// </remarks>
        /// <param name="ticket">A ticket</param>
        /// <returns>A clean ticket</returns>
        private static string CleanTicket(string ticket)
        {
            string[] separatingStrings = { "</D><D>", "</D></PT>" };

            var words = ticket.Split(separatingStrings, StringSplitOptions.None);
            var cleanedTicket = string.Empty;
            var first = true;
            foreach (var word in words)
            {
                if (first)
                {
                    cleanedTicket = word + "</D>";
                    first = false;
                }
                else
                {
                    // Convert special characters to character entities
                    cleanedTicket += "<D>" + SecurityElement.Escape(word) + "</D>";
                }
            }

            // Handle end of string
            return cleanedTicket.Substring(0, cleanedTicket.LastIndexOf("<D></D>", StringComparison.Ordinal)) + "</PT>";
        }

        public List<byte> AlignLine(int index, int ticketCharactersPerLine, IReadOnlyList<string> left, IReadOnlyList<string> center, IReadOnlyList<string> right)
        {
            var line = new List<byte>();
            var leftString = index < left.Count ? left[index] : string.Empty;
            var centerString = index < center.Count ? center[index] : string.Empty;
            var rightString = index < right.Count ? right[index] : string.Empty;

            // truncate left string if needed
            leftString = AdjustTextLength(leftString, ticketCharactersPerLine);
            centerString = AdjustTextLength(centerString, ticketCharactersPerLine);
            rightString = AdjustTextLength(rightString, ticketCharactersPerLine);

            var completeLine = new StringBuilder(); //This will have complete line which would be sent to printer to print.

            //if all of the left/centre/right has null/empty value
            if (string.IsNullOrEmpty(leftString) && string.IsNullOrEmpty(centerString) &&
                string.IsNullOrEmpty(rightString))
            {
                line.AddRange(" ".ToByteArray());
                return line;
            }

            /* LeftString | spaceCount1 | CentreString | spaceCount2 | RightString */

            completeLine.Append(leftString); //left string

            if (!string.IsNullOrEmpty(centerString))
            {
                var spaceCount = Math.Max(0, (int)Math.Floor((float)(ticketCharactersPerLine - (centerString.Length)) / 2) - completeLine.Length);
                completeLine.Append(' ', spaceCount);
                completeLine.Append(centerString);

                spaceCount = Math.Max(0, ticketCharactersPerLine - rightString.Length - completeLine.Length);
                completeLine.Append(' ', spaceCount); //space b/w centre and right string
                completeLine.Append(rightString); //space b/w centre and right string
            }
            else
            {
                var spaceCount = Math.Max(0, ticketCharactersPerLine - (leftString?.Length ?? 0) - rightString.Length);
                completeLine.Append(' ', spaceCount);
                completeLine.Append(rightString);
            }

            line.AddRange(AdjustTextLength(completeLine.ToString(), ticketCharactersPerLine).ToByteArray());

            return line;
        }

        public string AdjustTextLength(string text, int ticketCharactersPerLine)
        {
            const string newLine = "\n";
            // if just a newline, strip it off
            if (text == newLine)
            {
                return string.Empty;
            }

            return text.Length > ticketCharactersPerLine ? text.Substring(0, ticketCharactersPerLine) : text;
        }

        protected void UpdatePrinterSpecificTemplateMappingsForFirmware()
        {
            var count = MonoAddinsHelper.SelectedConfigurations?.Count ?? 0;
            Logger.Debug($"getting path to PrinterOverrides file. SelectedConfigurations count is {count}");
            var node = MonoAddinsHelper.GetSelectedNodes<FilePathExtensionNode>(PrinterOverridesExtensionPath);

            // node will be null if there aren't any printer overrides for the current jurisdiction
            if (node is null || node.Count == 0)
            {
                Logger.Debug($"no extension path found for {PrinterOverridesExtensionPath}");
                return;
            }

            var printerOverridesPath = node.First().FilePath;
            Logger.Debug($"Path to PrinterOverrides file is {printerOverridesPath}");
            PrinterOverrideParser.LoadOverrides(printerOverridesPath);
            PrinterSpecificTemplateMappings = PrinterOverrideParser.GetPrinterSpecificOverride(Protocol, FirmwareVersion);

            // update TemplateCache. need to replace existing items in the list with updated ones
            // this is done in DefineTemplates but we can load the overrides here
            _templateOverrides = PrinterOverrideParser.GetTemplateOverrides(Protocol, FirmwareVersion)?.ToList();
            if (_templateOverrides is null)
            {
                Logger.Debug($"No template overrides found");
            }
        }
    }
}
