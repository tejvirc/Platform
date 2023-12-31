﻿// ReSharper disable InconsistentlySynchronizedField
namespace Aristocrat.Monaco.Hardware.Serial.Printer.TCL
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Gds.Printer;
    using Contracts.Gds;
    using log4net;

    public abstract class TclProtocol : SerialPrinter
    {
        private const int PollingIntervalMs = 500;
        private const int PrintingPollingIntervalMs = 100;
        private const int TenSecondsMs = 10000;
        private const int ExpectedResponseTime = 50;
        private const int CrcResponseTime = 40000;
        private const int MinimumLinesPerTicket = 36;
        private const int TicketCharactersPerLine = 45;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly object Lock = new ();

        private bool _isPrinting;
        private bool _isValidationCompleteReported;
        private bool _isValidationFlagSet;
        private bool _waitForRegionOfInterest;
        private bool _selfTestDone;
        private bool _isTopOfForm;
        private bool _isTemplateMode;

        private byte _currentRegionId = TclProtocolConstants.UndefinedPrintAreaIdCharacter;
        private byte _currentTemplateId = TclProtocolConstants.UndefinedPrintAreaIdCharacter;
        private CancellationToken _cancellationToken;

        private readonly Dictionary<string, PrintableObject> _regionCommands = new ();
        private readonly Dictionary<int, PrintableObject> _templateCommands = new ();
        private readonly HashSet<string> _loadedRegions = new ();
        private readonly HashSet<int> _loadedTemplates = new ();

        // Map from GDS PDL rotation codes to TCL ones
        private static readonly Dictionary<GdsConstants.PdlRotation, TclProtocolConstants.TclPrintDirection> RotationMap =
            new ()
            {
                { GdsConstants.PdlRotation.None, TclProtocolConstants.TclPrintDirection.Right },
                { GdsConstants.PdlRotation.Quarter, TclProtocolConstants.TclPrintDirection.Down },
                { GdsConstants.PdlRotation.Half, TclProtocolConstants.TclPrintDirection.Left },
                { GdsConstants.PdlRotation.ThreeQuarters, TclProtocolConstants.TclPrintDirection.Up }
            };

        // Map from GDS PDL justification codes to TCL ones
        private static readonly Dictionary<GdsConstants.PdlJustify, TclProtocolConstants.TclPrintJustification> JustificationMap =
            new()
            {
                { GdsConstants.PdlJustify.Left, TclProtocolConstants.TclPrintJustification.Left },
                { GdsConstants.PdlJustify.Center, TclProtocolConstants.TclPrintJustification.Center },
                { GdsConstants.PdlJustify.Right, TclProtocolConstants.TclPrintJustification.Right }
            };

        private static bool _templateSent;
        private static bool _sendingTemplates;

        protected TclProtocol()
        {
            PollIntervalMs = PollingIntervalMs; // A sufficient polling rate
            CommunicationTimeoutMs = ExpectedResponseTime; // determined by observation, not spec
            MinimumResponseTime = ExpectedResponseTime;
            UseSyncMode = true;
        }

        ///  <inheritdoc />
        public override bool Open()
        {
            EnablePolling(false);

            try
            {
                return base.Open() && RequestStatus();
            }
            finally
            {
                EnablePolling(true);
            }
        }

        /// <summary>
        ///     Gets a value specifying the width of the template in dots.
        /// </summary>
        protected abstract int TemplateWidthDots { get; }

        /// <summary>
        ///     Gets a value specifying the length of the template in dots.
        /// </summary>
        protected abstract int TemplateLengthDots { get; }

        /// <summary>
        ///     Gets a collection of all region ids for regions that contain a barcode.
        /// </summary>
        /// <returns>The barcode regions ids.</returns>
        protected HashSet<string> BarcodeRegionIds { get; } = new HashSet<string>();

        /// <summary>
        ///     Parses the CRC response to extract the CRC data.
        /// </summary>
        /// <param name="response">The response message.</param>
        /// <returns>The CRC data.</returns>
        protected abstract int ParseCrcResponse(byte[] response);

        /// <summary>
        ///     Gets a collection of the indexes of regions that contain a barcode for a given template.
        /// </summary>
        /// <returns>A dictionary of template ids and the barcode region indexes.</returns>
        protected Dictionary<int, HashSet<int>> TemplateBarcodeRegionIndexes { get; } = new Dictionary<int, HashSet<int>>();

        /// <summary>
        ///     Calculates the dimensions of the region based to the requirements of this device.
        /// </summary>
        /// <param name="regionData">The TCL region data.</param>
        /// <param name="region">The PDL region data.</param>
        protected abstract void CalculateRegionDimensions(ref TclRegionData regionData, dprtype region);

        /// <summary>
        ///     Get device identification information the the selected implementation.
        /// </summary>
        /// <returns>Whether or not the device information was successfully read from the device</returns>
        protected abstract bool GetDeviceSpecificInformation();

        /// <summary>
        /// Gets the GDS equivalent font for this device.
        /// </summary>
        /// <param name="gdsFontNumber">The GDS font number</param>
        /// <returns>The GDS equivalent font for this device</returns>
        protected abstract string GetMappedFont(int gdsFontNumber);

        /// <summary>
        ///     Pre-renders region data before final rendering to be sent to printer.
        ///     This allows individual device drivers to format data as needed.
        /// </summary>
        /// <param name="pdlTemplateId">The PDL template id.</param>
        /// <param name="printData">The print data.</param>
        /// <returns>The pre-rendered print data.</returns>
        protected abstract List<PrintDataField> PreRenderPrintRegions(int pdlTemplateId, List<PrintDataField> printData);

        /// <summary>
        ///     Pre-renders template region data before final rendering to be sent to printer.
        ///     This allows individual device drivers to format data as needed.
        /// </summary>
        /// <param name="regionIds">The region ids data.</param>
        /// <returns>The pre-rendered region data.</returns>
        protected abstract List<string> PreRenderTemplateRegions(List<string> regionIds);

        ///  <inheritdoc />
        protected override void CalculateCrc()
        {
            if (FirmwareCrc != UnknownCrc)
            {
                return;
            }

            if (!RequestStatus())
            {
                return;
            }

            EnableTemplatePrintingMode();

            // This operation can take up to 40 seconds.
            AdjustReceiveTimeout(CrcResponseTime);
            MinimumResponseTime = CrcResponseTime;

            lock (Lock)
            {
                Logger.Debug("send CalculateCrc");
                var response = SendMessage(
                    TclProtocolConstants.ReadCrcCommand,
                    TclProtocolConstants.CrcResponseLength);

                FirmwareCrc = ParseCrcResponse(response);

                if (FirmwareCrc != UnknownCrc)
                {
                    OnMessageReceived(new CrcData
                    {
                        Result = FirmwareCrc
                    });
                }
            }

            MinimumResponseTime = ExpectedResponseTime;
            AdjustReceiveTimeout(CommunicationTimeoutMs);
        }

        ///  <inheritdoc />
        protected override Task<bool> FormFeed()
        {
            lock (Lock)
            {
                Logger.Debug("send FormFeed");
                ResetPrintingFlags(false);
                EnableTemplatePrintingMode();
                SendMessage(TclProtocolConstants.FormFeedSingleCommand);
            }

            // there is no response so we can only presume it worked
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        protected override Task<bool> DefineRegion(dprtype region)
        {
            lock (Lock)
            {
                RegionCache.Add(region);
            }

            Logger.Debug($"DefineRegion: UsePrinterDefinedTemplates is {UsePrinterDefinedTemplates} Firmware is '{FirmwareVersion}' Protocol is '{Protocol}'");
            CheckForNewRegionsAndTemplates();
            return Task.FromResult(UsePrinterDefinedTemplates || RenderRegionDefinition(region));
        }

        ///  <inheritdoc />
        protected override void Enable(bool enable)
        {
            if (!enable)
            {
                AbortPrintBatch();
            }
        }

        ///  <inheritdoc />
        protected override bool GetDeviceInformation()
        {
            lock (Lock)
            {
                RequestStatus();
                var response = GetDeviceSpecificInformation();

                // at this point we know the protocol and firmware version
                Logger.Debug($"calling UpdatePrinterSpecificTemplateMappingsForFirmware with Protocol '{Protocol}' and FirmwareVersion '{FirmwareVersion}'");
                UpdatePrinterSpecificTemplateMappingsForFirmware();
                DeleteAllRegionsAndTemplates();

                return response;
            }
        }

        ///  <inheritdoc />
        protected override Task<bool> PrintTicket(PrintCommand ticket, CancellationToken token)
        {
            _cancellationToken = token;
            EnablePolling(false);
            var result = RequestStatus() && RenderPrintData(ticket);
            EnablePolling(true);

            return Task.FromResult(result);
        }

        ///  <inheritdoc />
        protected override bool RequestStatus()
        {
            lock (Lock)
            {
                var status = RequestStatusInternal();
                if (status == 0)
                {
                    return false;
                }

                _isTemplateMode = !status.HasFlag(TclProtocolConstants.TclStatus.JournalPrinting);
                _isValidationFlagSet = status.HasFlag(TclProtocolConstants.TclStatus.ValidationNumberDone)  ||
                                       status.HasFlag(TclProtocolConstants.TclStatus.BarcodeDataIsAccessed);
                _isTopOfForm = status.HasFlag(TclProtocolConstants.TclStatus.TopOfForm);
                var isError = status.HasFlag(TclProtocolConstants.TclStatus.SystemError);
                var isCommandError = status.HasFlag(TclProtocolConstants.TclStatus.CommandError);

                Logger.Debug($"Status: Printing={_isPrinting}, ValidationCompleteReported={_isValidationCompleteReported}, ValidationFlagSet={_isValidationFlagSet} in template mode={_isTemplateMode} self test done={_selfTestDone} System Error={isError} Command Error={isCommandError}");

                if (_isPrinting && !_isValidationCompleteReported && _isValidationFlagSet)
                {
                    _isValidationCompleteReported = true;
                    _waitForRegionOfInterest = false;
                    TicketPrintStatus = new TicketPrintStatus
                    {
                        FieldOfInterest1 = true,
                        PrintInProgress = _isPrinting
                    };
                }

                CheckSelfTestDone(status);

                Logger.Debug($"isPrinting={_isPrinting} waitingForRegionOfInterest={_waitForRegionOfInterest} isTemplateMode={_isTemplateMode} busy={status.HasFlag(TclProtocolConstants.TclStatus.Busy)}");

                if (_isPrinting && !status.HasFlag(TclProtocolConstants.TclStatus.Busy) && !_waitForRegionOfInterest && _isTemplateMode)
                {
                    _isPrinting = false;
                    TicketPrintStatus = new TicketPrintStatus { PrintComplete = true };
                    UpdatePollingRate(PollingIntervalMs);
                }
                else if (_cancellationToken.IsCancellationRequested)
                {
                    _isPrinting = false;
                    _cancellationToken = default;
                }

                return true;
            }
        }

        ///  <inheritdoc />
        protected override void SelfTest()
        {
            Logger.Debug("Starting self test");
            EnablePolling(false);
            RequestStatus();
            CalculateCrc();

            if (_isValidationFlagSet && !_isTopOfForm)
            {
                FormFeed();
            }

            ClearOnlineErrors();

            _selfTestDone = true;
            EnablePolling(true);

            OnMessageReceived(new FailureStatus());
        }

        private void CheckSelfTestDone(TclProtocolConstants.TclStatus status)
        {
            if (_selfTestDone)
            {
                PrinterStatus = new PrinterStatus
                {
                    PaperInChute = !_isPrinting && status.HasFlag(TclProtocolConstants.TclStatus.PaperInPath), // Paper In Chute only matters when not printing.
                    PaperLow = status.HasFlag(TclProtocolConstants.TclStatus.PaperLow),
                    // The printer will continue to print when the chassis is open, we only flag the chasis open if we are not
                    // printing or printing and not waiting for a region of interest. 
                    ChassisOpen = (!_isPrinting || _isPrinting && !_waitForRegionOfInterest)
                                  && status.HasFlag(TclProtocolConstants.TclStatus.ChassisIsOpen),
                    PrintHeadOpen = status.HasFlag(TclProtocolConstants.TclStatus.HeadIsUp),
                    PaperJam = status.HasFlag(TclProtocolConstants.TclStatus.PaperJam),
                    PaperEmpty = !_isPrinting &&
                                 !status.HasFlag(TclProtocolConstants.TclStatus.ChassisIsOpen) &&
                                 !status.HasFlag(TclProtocolConstants.TclStatus.HeadError) &&
                                 status.HasFlag(TclProtocolConstants.TclStatus.OutOfPaper), // Paper Out only matters when not printing.
                    TopOfForm = _isPrinting ||
                                status.HasFlag(TclProtocolConstants.TclStatus.TopOfForm) || // Top-of-form only matters at certain times.
                                status.HasFlag(TclProtocolConstants.TclStatus.OutOfPaper) ||
                                !status.HasFlag(TclProtocolConstants.TclStatus.HeadError)
                };

                if (_isPrinting && HasDisablingFault)
                {
                    Logger.Warn($"RequestStatus - HasDisablingFault - _waitForRegionOfInterest {_waitForRegionOfInterest}");
                    if (_waitForRegionOfInterest)
                    {
                        SendMessage(TclProtocolConstants.AbortBarcodePrintCommand);
                    }

                    _isPrinting = false;
                }
            }
        }

        private void AbortPrintBatch()
        {
            SendMessage(TclProtocolConstants.FlushAllCommand);
        }

        private void AppendTclSegmentGroup(StringBuilder command, string value)
        {
            SpecialCaseCharacterReplacement(ref value);

            command.Append(value);
            command.Append(TclProtocolConstants.GroupSeparatorCode);
        }

        private void AppendTclSegmentGroup(StringBuilder command, int value)
        {
            AppendTclSegmentGroup(command, value.ToString());
        }

        private void CacheBarcodeRegionIndexes(int pdlTemplateId, List<string> regionIds)
        {
            if (TemplateBarcodeRegionIndexes.ContainsKey(pdlTemplateId))
            {
                return;
            }

            TemplateBarcodeRegionIndexes.Add(pdlTemplateId, new HashSet<int>());
            for (var i = 0; i < regionIds.Count; i++)
            {
                if (BarcodeRegionIds.Contains(regionIds[i]))
                {
                    TemplateBarcodeRegionIndexes[pdlTemplateId].Add(i);
                }
            }
        }

        private void ClearOnlineErrors()
        {
            EnableTemplatePrintingMode();
            SendMessage(TclProtocolConstants.ClearOnlineErrorsCommand);
        }

        private string CreatePrintCommand(string templateId, List<PrintDataField> printData)
        {
            Logger.Debug($"Creating print command for template '{templateId}'");
            var command = new StringBuilder(TclProtocolConstants.TransmitCode);

            AppendTclSegmentGroup(command, TclProtocolConstants.PrintCode);
            AppendTclSegmentGroup(command, templateId);
            AppendTclSegmentGroup(command, TclProtocolConstants.PrintOneCopyCode);

            foreach (var data in printData)
            {
                var adjustedData = data.Data?.Replace("~", "~126").Replace("^", "~094").Replace("|", "~124");
                AppendTclSegmentGroup(command, adjustedData);
            }

            command.Append(TclProtocolConstants.TransmitCode);
            Logger.Debug($"print command is {command}");
            return command.ToString();
        }

        private PrintableObject CreatePrinterRegion(string regionId, TclRegionData regionData)
        {
            // Command format:
            // ^R|<r_id>|<targ_mem>|<da_start>|<pa_start>|
            // <da_len>|<pa_len>|<unused1>|<rot>|<just>|<obj_id>|
            // <mul_1>|<mul_2>|<obj_att>|<pr_att>|<pr_data>|^

            var command = new StringBuilder(TclProtocolConstants.TransmitCode);

            AppendTclSegmentGroup(command, TclProtocolConstants.RegionCode);
            AppendTclSegmentGroup(command, regionId);
            AppendTclSegmentGroup(command, TclProtocolConstants.RamMemoryTargetCode);
            AppendTclSegmentGroup(command, regionData.Origin.X);
            AppendTclSegmentGroup(command, regionData.Origin.Y);
            AppendTclSegmentGroup(command, regionData.Size.Width);
            AppendTclSegmentGroup(command, regionData.Size.Height);
            AppendTclSegmentGroup(command, string.Empty); // Unused
            AppendTclSegmentGroup(command, (int)regionData.Direction);
            AppendTclSegmentGroup(command, (int)regionData.Justification);

            AppendTclSegmentGroup(
                command,
                regionData.IsBarcode
                    ? TclProtocolConstants.BarcodeInterleaved2Of5Code
                    : regionData.FontNumber);

            AppendTclSegmentGroup(command, regionData.Multiplier1);
            AppendTclSegmentGroup(command, regionData.Multiplier2);
            AppendTclSegmentGroup(command, regionData.ObjectAttribute);
            AppendTclSegmentGroup(command, TclProtocolConstants.PrintAttributeDynamicTextCode);
            AppendTclSegmentGroup(command, string.Empty); // Unused with dynamic data

            command.Append(TclProtocolConstants.TransmitCode);

            return new PrintableObject
            {
                Id = regionId,
                Command = command.ToString()
            };
        }

        private PrintableObject CreatePrinterTemplate(string templateId, List<string> regionIds)
        {
            // Command format:
            // ^T|<t_id>|<targ_mem>|<t_dim_da>|<t_dim_pa>|<pr#1>|<pr#2>|...|<pr#n>|^
            var command = new StringBuilder(TclProtocolConstants.TransmitCode);

            AppendTclSegmentGroup(command, TclProtocolConstants.TemplateCode);
            AppendTclSegmentGroup(command, templateId);
            AppendTclSegmentGroup(command, TclProtocolConstants.RamMemoryTargetCode);
            AppendTclSegmentGroup(command, TemplateWidthDots);
            AppendTclSegmentGroup(command, TemplateLengthDots);

            lock (Lock)
            {
                foreach (var regionId in regionIds)
                {
                    AppendTclSegmentGroup(command, _regionCommands[regionId].Id);
                }
            }

            command.Append(TclProtocolConstants.TransmitCode);

            return new PrintableObject
            {
                Id = templateId,
                Command = command.ToString(),
                RegionCount = regionIds.Count
            };
        }

        private void DeletePrinterTemplate(string templateId)
        {
            // Command format:
            // ^T|<t_id>|DR|^

            var command = new StringBuilder(TclProtocolConstants.TransmitCode);
            AppendTclSegmentGroup(command, TclProtocolConstants.TemplateCode);
            AppendTclSegmentGroup(command, templateId);
            AppendTclSegmentGroup(command, "DR");
            command.Append(TclProtocolConstants.TransmitCode);
            SendMessage(command.ToString());
        }

        private void DeletePrinterRegion(string regionId)
        {
            // Command format:
            // ^R|<r_id>|DR|^

            var command = new StringBuilder(TclProtocolConstants.TransmitCode);
            AppendTclSegmentGroup(command, TclProtocolConstants.RegionCode);
            AppendTclSegmentGroup(command, regionId);
            AppendTclSegmentGroup(command, "DR");
            command.Append(TclProtocolConstants.TransmitCode);
            SendMessage(command.ToString());
        }

        private void DeleteAllRegionsAndTemplates()
        {
            lock (Lock)
            {
                RegionCache.Clear();
                TemplateCache.Clear();

                if (UsePrinterDefinedTemplates)
                {
                    return;
                }

                _loadedRegions.Clear();
                _loadedTemplates.Clear();
                SendMessage(TclProtocolConstants.DeleteAllRegionsCommand);
                ClearOnlineErrors();
            }
        }

        private void EnableTemplatePrintingMode()
        {
            Logger.Debug("enabling template printing mode");
            lock (Lock)
            {
                if (!_isTemplateMode)
                {
                    SendMessage(TclProtocolConstants.EnableTemplatePrintingModeCommand, TclProtocolConstants.EmptyResponseLength);
                }
            }
        }

        private void EnableLinePrintingMode()
        {
            Logger.Debug("Going to line printer mode");
            lock (Lock)
            {
                if (_isTemplateMode)
                {
                    SendMessage(TclProtocolConstants.EnableLinePrintingModeCommand);
                }
            }
        }

        private static string GetNextPrintableObjectId(ref byte currentId)
        {
            // Returns characters '0' through '}' (0x30-0x7D) excluding '=', '^' (0x5E) and '|' (0x7C)
            // } will be repeated because we can not have more ids than this
            // If a character is used more than once it will be handled by a PRData error
            currentId++;
            if (char.IsLetterOrDigit((char)currentId))
            {
                return ((char)currentId).ToString();
            }

            switch (currentId)
            {
                case (byte)':':
                    currentId = (byte)'A';
                    break;
                case (byte)'[':
                    currentId = (byte)'a';
                    break;
                case (byte)'{':
                    currentId = (byte)'z';  // error case if we use all the region ids
                    break;
            }

            return ((char)currentId).ToString();
        }

        private bool MatchResponseStart(byte[] response, byte[] responseStart)
        {
            return response != null &&
                   responseStart != null &&
                   responseStart.Length <= response.Length &&
                   CompareByteArrays(response, responseStart, responseStart.Length) == 0;
        }

        private bool RenderPrintData(PrintCommand ticket)
        {
            Logger.Debug($"UsePrinterDefinedTemplates is {UsePrinterDefinedTemplates}");
            return UsePrinterDefinedTemplates
                    ? RenderPrinterDefinedPrintData(ticket)
                    : RenderPlatformDefinedPrintData(ticket);
        }

        private bool RenderPrinterDefinedPrintData(PrintCommand ticket)
        {
            Logger.Debug($"RenderPrinterDefinedTemplate with ticket '{ticket.Id}' templateSent is '{_templateSent}' RegionCache has '{RegionCache.Count}' items");
            lock (Lock)
            {
                if (!CheckForNewRegionsAndTemplates())
                {
                    return false;
                }

                if (PrinterSpecificTemplateMappings.IsAuditTicket(ticket))
                {
                    return Render3ColumnTicketData(ticket);
                }

                var remappedTicket =
                    PrinterSpecificTemplateMappings.RemapPrintCommand(ticket, TemplateCache, RegionCache);
                Logger.Debug($"Remapped ticket is '{remappedTicket.PrinterTemplateId}' ticket id is '{ticket.Id}' TemplateCache has '{TemplateCache.Count}' items. RegionCache has '{RegionCache.Count}' items");
                var command = CreatePrintCommand(
                    remappedTicket.PrinterTemplateId,
                    remappedTicket.DataFields.ToList());
                Logger.Debug($"Tcl command is {command}");
                var hasRegionOfInterest = ticket.DataFields.Any(x => x.IsRegionOfInterest > 0);
                return SendPrintMessage(command, hasRegionOfInterest);
            }
        }

        private bool CheckForNewRegionsAndTemplates()
        {
            if (_templateSent || _sendingTemplates)
            {
                return true;
            }

            _sendingTemplates = true;

            var overrides = PrinterOverrideParser.GetNewTemplates(Protocol, FirmwareVersion);

            // check if there are new regions defined in the override
            var newRegions = overrides?.NewRegion?.ToList();
            ClearOnlineErrors();
            if (newRegions is not null)
            {
                foreach (var newRegion in newRegions)
                {
                    ClearOnlineErrors();
                    DeletePrinterRegion(newRegion.PrinterRegionId);
                    if (WaitUntilPrinterNotBusy().HasFlag(TclProtocolConstants.TclStatus.SystemError))
                    {
                        // may get an error if trying to delete a non-existent region, so ignore errors here
                        Logger.Error("Error while deleting existing region");
                        ClearOnlineErrors();
                    }

                    Logger.Debug($"Sending region from override. Region={newRegion.Command}");
                    SendTemplate(newRegion.Command.ToByteArray());
                    if (WaitUntilPrinterNotBusy().HasFlag(TclProtocolConstants.TclStatus.SystemError))
                    {
                        Logger.Error("Error while sending new region");
                        return false;
                    }
                }
            }

            // check if there are new templates defined in the override
            var newTemplates = overrides?.NewTemplate?.ToList();
            if (newTemplates is not null)
            {
                foreach (var newTemplate in newTemplates)
                {
                    ClearOnlineErrors();
                    DeletePrinterTemplate(newTemplate.PrinterTemplateId);
                    if (WaitUntilPrinterNotBusy().HasFlag(TclProtocolConstants.TclStatus.SystemError))
                    {
                        // may get an error if deleting a non-existent template, so ignore errors here
                        Logger.Error("Error while deleting template");
                        ClearOnlineErrors();
                    }

                    Logger.Debug($"Sending template from override. Template={newTemplate.Command}");
                    SendTemplate(newTemplate.Command.ToByteArray());
                    if (WaitUntilPrinterNotBusy().HasFlag(TclProtocolConstants.TclStatus.SystemError))
                    {
                        Logger.Error("Error while sending new template");
                        return false;
                    }
                }
            }

            _templateSent = true;
             return true;
        }

        private bool RenderPlatformDefinedPrintData(PrintCommand ticket)
        {
            lock (Lock)
            {
                if (!_templateCommands.ContainsKey(ticket.Id))
                {
                    Logger.Debug($"RenderPrintData can't find ticket ID {ticket.Id}");
                    return false;
                }

                if (PrinterSpecificTemplateMappings is not null && PrinterSpecificTemplateMappings.IsAuditTicket(ticket))
                {
                    return Render3ColumnTicketData(ticket);
                }

                if (_templateCommands[ticket.Id].RegionCount < ticket.DataFields.Length)
                {
                    Logger.Debug("RenderPrintData not enough region definitions");
                }

                var regionPrintData = ticket.DataFields.ToList();
                regionPrintData = PreRenderPrintRegions(ticket.Id, regionPrintData);

                var templateId = _templateCommands[ticket.Id].Id;
                var command = CreatePrintCommand(templateId, regionPrintData);

                var hasRegionOfInterest = ticket.DataFields.Any(x => x.IsRegionOfInterest > 0);
                return SendPrintMessage(command, hasRegionOfInterest);
            }
        }

        protected override bool RenderRegionDefinition(dprtype pdlRegion)
        {
            lock (Lock)
            {
                Logger.Debug($"RenderRegionDefinition: region {pdlRegion.id} regionCommands has {_regionCommands.Count} entries");
                if (!_regionCommands.ContainsKey(pdlRegion.id))
                {
                    var typeParts = pdlRegion.type.Split('=');

                    var isBarcode = (GdsConstants.PdlRegionType)typeParts[0][0] == GdsConstants.PdlRegionType.Barcode;
                    var gdsFontNumber = int.Parse(typeParts[1]);

                    var fontNumber = PrinterSpecificTemplateMappings.GetSpecificFont(gdsFontNumber.ToString(), pdlRegion.id);

                    if (string.IsNullOrEmpty(fontNumber))
                    {
                        fontNumber = GetMappedFont(gdsFontNumber);
                    }

                    var gdsRotation = (GdsConstants.PdlRotation)int.Parse(pdlRegion.rot);
                    var rotation = RotationMap[gdsRotation];

                    var gdsJustification = (GdsConstants.PdlJustify)pdlRegion.jst;
                    var justification = JustificationMap[gdsJustification];

                    var attribute = int.Parse(pdlRegion.attr);
                    if (!isBarcode)
                    {
                        // TCL font attributes are 1 less than GDS
                        attribute--;
                    }

                    var regionData = new TclRegionData
                    {
                        FontNumber = fontNumber,
                        Direction = rotation,
                        Justification = justification,
                        Multiplier1 = pdlRegion.m1,
                        Multiplier2 = pdlRegion.m2,
                        ObjectAttribute = attribute,
                        IsBarcode = isBarcode
                    };

                    if (regionData.IsBarcode)
                    {
                        BarcodeRegionIds.Add(pdlRegion.id);
                    }

                    CalculateRegionDimensions(ref regionData, pdlRegion);

                    var regionId = GetNextPrintableObjectId(ref _currentRegionId);
                    var printerRegion = CreatePrinterRegion(regionId, regionData);
                    _regionCommands[pdlRegion.id] = printerRegion;
                }

                var success = true;
                if (!_loadedRegions.Contains(pdlRegion.id))
                {
                    ClearOnlineErrors();
                    Logger.Debug($"RenderRegionDefinition: sending region {pdlRegion.id} to printer");

                    SendMessage(_regionCommands[pdlRegion.id].Command);

                    success = !WaitUntilPrinterNotBusy().HasFlag(TclProtocolConstants.TclStatus.DataError);

                    if (success)
                    {
                        _loadedRegions.Add(pdlRegion.id);
                    }
                    else
                    {
                        Logger.Debug("Error sending command");
                        ClearOnlineErrors();
                    }
                }

                Logger.Debug($"RenderRegionDefinition: returning {success}");
                return success;
            }
        }

        protected override bool RenderTemplateDefinition(dpttype pdlTemplate)
        {
            lock (Lock)
            {
                Logger.Debug($"RenderTemplateDefinition: template is {pdlTemplate.id} templateCommands has {_templateCommands.Count} elements");
                if (!_templateCommands.ContainsKey(pdlTemplate.id))
                {
                    var templateId = GetNextPrintableObjectId(ref _currentTemplateId);
                    var regionIds = new List<string>(
                        pdlTemplate.Value.Split(new[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries));

                    CacheBarcodeRegionIndexes(pdlTemplate.id, regionIds);

                    regionIds = PreRenderTemplateRegions(regionIds);
                    var printerTemplate = CreatePrinterTemplate(templateId, regionIds);
                    _templateCommands[pdlTemplate.id] = printerTemplate;
                }

                var success = true;
                if (!_loadedTemplates.Contains(pdlTemplate.id))
                {
                    Logger.Debug($"RenderTemplateDefinition: sending template {pdlTemplate.id} to printer");
                    ClearOnlineErrors();
                    SendMessage(_templateCommands[pdlTemplate.id].Command);
                    success = !WaitUntilPrinterNotBusy().HasFlag(TclProtocolConstants.TclStatus.DataError);

                    if (success)
                    {
                        Logger.Debug("adding template to loadedTemplates");
                        _loadedTemplates.Add(pdlTemplate.id);
                    }
                }

                return success;
            }
        }

        private TclProtocolConstants.TclStatus WaitUntilPrinterNotBusy()
        {
            var delayCount = 5;
            var status = RequestStatusInternal();
            while (delayCount > 0 && status.HasFlag(TclProtocolConstants.TclStatus.Busy))
            {
                delayCount--;
                Thread.Sleep(100);
                status = RequestStatusInternal();
            }

            return status;
        }

        private TclProtocolConstants.TclStatus RequestStatusInternal()
        {
            lock (Lock)
            {
                Logger.Debug("send RequestStatus");
                var response = SendMessage(
                    TclProtocolConstants.RequestStatusCommand,
                    TclProtocolConstants.StatusResponseLength);

                if (response == null || (response.Length > 1 && response[1] != TclProtocolConstants.StatusCharacter))
                {
                    return 0;
                }

                Logger.Debug($"response is {BitConverter.ToString(response)}");
                FirmwareVersion = Encoding.ASCII.GetString(
                    response,
                    TclProtocolConstants.VersionOffset,
                    TclProtocolConstants.VersionLength);

                var statusBits = response[TclProtocolConstants.StatusOffset] +                     // status1
                                 ((long)response[TclProtocolConstants.StatusOffset + 2] << 8) +    // status2
                                 ((long)response[TclProtocolConstants.StatusOffset + 4] << 16) +   // status3
                                 ((long)response[TclProtocolConstants.StatusOffset + 6] << 24) +   // status4
                                 ((long)response[TclProtocolConstants.StatusOffset + 8] << 32);    // status5

                return (TclProtocolConstants.TclStatus)statusBits;
            }
        }

        private void ResetPrintingFlags(bool hasRegionOfInterest)
        {
            ClearOnlineErrors();

            lock (Lock)
            {
                _waitForRegionOfInterest = hasRegionOfInterest;
                _isValidationCompleteReported = false;
                _isPrinting = true;
            }
        }

        private void SendTemplate(byte[] templateMessage)
        {
            lock (Lock)
            {
                EnableTemplatePrintingMode();

                SendMessage(templateMessage, TclProtocolConstants.EmptyResponseLength);

                UpdatePollingRate(PrintingPollingIntervalMs);
            }
        }

        private bool SendPrintMessage(string printCommand, bool hasRegionOfInterest)
        {
            lock (Lock)
            {
                ResetPrintingFlags(hasRegionOfInterest);
                EnableTemplatePrintingMode();

                TicketPrintStatus = new TicketPrintStatus { PrintInProgress = true };
                AdjustWriteTimeout(TenSecondsMs);

                Logger.Debug("send ticket command");
                SendMessage(printCommand);

                AdjustWriteTimeout(CommunicationTimeoutMs);
                UpdatePollingRate(PrintingPollingIntervalMs);

                return true;
            }
        }

        protected bool SendPrintMessage(byte[] printMessage, bool hasRegionOfInterest)
        {
            lock (Lock)
            {
                ResetPrintingFlags(hasRegionOfInterest);
                EnableLinePrintingMode();

                TicketPrintStatus = new TicketPrintStatus { PrintInProgress = true };
                AdjustWriteTimeout(TenSecondsMs);

                Logger.Debug("send ticket command");
                SendMessage(printMessage, TclProtocolConstants.EmptyResponseLength);

                AdjustWriteTimeout(CommunicationTimeoutMs);
                UpdatePollingRate(PrintingPollingIntervalMs);

                return true;
            }
        }

        private void SendMessage(string message)
        {
            var originalTaskPriority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            try
            {
                Logger.Debug($"sending printer command {message}");
                SendCommand(message.ToByteArray());
                WaitSendComplete();
            }
            finally
            {
                Thread.CurrentThread.Priority = originalTaskPriority;
            }
        }

        protected byte[] SendMessage(byte[] data, int responseLength)
        {
            byte[] response = null;
            var originalTaskPriority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            try
            {
                // Some commands don't get a response
                if (responseLength == 0)
                {
                    SendCommand(data);
                    WaitSendComplete();
                }
                else
                {
                    response = SendCommandAndGetResponse(data, responseLength);

                    // Commands that get a response, sometimes expect certain exact bytes at the start of the response
                    if (!MatchResponseStart(response, TclProtocolConstants.EmptyResponse))
                    {
                        Logger.Error($"Unknown response {response} or timeout");
                        response = null;
                    }
                }
                return response;
            }
            finally
            {
                Thread.CurrentThread.Priority = originalTaskPriority;
            }
        }

        protected virtual bool Render3ColumnTicketData(PrintCommand ticket)
        {
            Logger.Debug("Rendering 3 column ticket in line mode");
            string[] header = { ticket.DataFields[3].Data ?? string.Empty };

            var leftColumn = ticket.DataFields[0].Data;
            NotSupportedCharacterReplacement(ref leftColumn);

            var centerColumn = ticket.DataFields[1].Data;
            NotSupportedCharacterReplacement(ref centerColumn);

            var rightColumn = ticket.DataFields[2].Data;
            NotSupportedCharacterReplacement(ref rightColumn);

            var builder = new List<byte>();
            // send printer initialize command
            builder.AddRange(TclProtocolConstants.CommandInitialize);
            // send unit of measure 1/180"
            builder.AddRange(TclProtocolConstants.CommandSetUnitOfMeasure);
            // set 20.5 cpi font
            builder.AddRange(TclProtocolConstants.CommandSet20_5CharactersPerInch);
            // set 1/8" line spacing
            builder.AddRange(TclProtocolConstants.CommandSetLineSpacing);

            // print the header text center aligned
            builder.AddRange(MakeLine(0, Array.Empty<string>(), header, Array.Empty<string>()));

            // add CR LF to move to next line
            builder.AddRange(TclProtocolConstants.CrLf);

            // split column strings into lines
            var splitLeft = leftColumn is null ? new string[] { } : leftColumn.Split('\n');
            var splitCenter = centerColumn is null ? new string[] { } : centerColumn.Split('\n');
            var splitRight = rightColumn is null ? new string[] { } : rightColumn.Split('\n');

            //we need to add CrLf;(to move to end of line), otherwise there would be different issue with different
            //firmwares like printer becomes disabled after printing 2nd page or 2nd page comes as empty page.
            int[] lengthArray = { splitLeft.Length, splitRight.Length, splitCenter.Length, GetMinimumNoLinesToPrint() };

            // loop thru all the column entries and create lines
            for (var i = 0; i < lengthArray.Max(); i++)
            {
                builder.AddRange(MakeLine(i, splitLeft, splitCenter, splitRight));
            }

            builder.Add(TclProtocolConstants.Cr);

            // switch back to template mode so ticket ejects fully
            builder.AddRange(TclProtocolConstants.EnableTemplatePrintingModeCommand);

            /* This is mandatory for the firmware GURSWFAA1(JCM Gen2 for Switzerland)
             otherwise printer would got disabled after printing the ticket while switching from line
             mode to template mode from above command*/

            builder.Add(TclProtocolConstants.Ff); //end the page with FF command

            // send template mode form feed
            builder.AddRange(TclProtocolConstants.FormFeedSingleCommand.ToByteArray());

            return SendPrintMessage(builder.ToArray(), false);
        }

        private byte[] MakeLine(int index, IReadOnlyList<string> left, IReadOnlyList<string> center, IReadOnlyList<string> right)
        {
            var line = new List<byte>();
            line.AddRange(AlignLine(index, TicketCharactersPerLine, left, center, right));
            line.AddRange(TclProtocolConstants.CrLf);
            return line.ToArray();
        }

        protected virtual int GetMinimumNoLinesToPrint()
        {
            return MinimumLinesPerTicket;
        }

        /// <summary>
        ///     Data for rendered printable objects
        /// </summary>
        public struct PrintableObject
        {
            /// <summary>
            ///     The TCL command to create this object
            /// </summary>
            public string Command;

            /// <summary>
            ///     The Id of this object
            /// </summary>
            public string Id;

            /// <summary>
            ///     The number of regions in this object. Only needed for templates.
            /// </summary>

            public int RegionCount;
        }

        /// <summary>
        ///     Data used to assemble a TCL region creation command
        /// </summary>
        public struct TclRegionData
        {
            /// <summary>
            ///     Text direction implies different uses of the coordinates.
            /// </summary>
            public TclProtocolConstants.TclPrintDirection Direction;

            /// <summary>
            ///     The font number
            /// </summary>
            public string FontNumber;

            /// <summary>
            ///     A value indicating if this region is a barcode.
            /// </summary>
            public bool IsBarcode;

            /// <summary>
            ///     The justification of the text.
            /// </summary>
            public TclProtocolConstants.TclPrintJustification Justification;

            /// <summary>
            ///     The first print object multiplier.
            /// </summary>
            public int Multiplier1;

            /// <summary>
            ///     The second print object multiplier.
            /// </summary>
            public int Multiplier2;

            /// <summary>
            ///     The object printing attribute.
            /// </summary>
            public int ObjectAttribute;

            /// <summary>
            ///     The origin of the region.
            /// </summary>
            public Point Origin;

            /// <summary>
            ///     The size of the region.
            /// </summary>
            public Size Size;
        }
    }
}
