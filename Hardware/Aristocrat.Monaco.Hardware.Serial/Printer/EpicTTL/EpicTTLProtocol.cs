namespace Aristocrat.Monaco.Hardware.Serial.Printer.EpicTTL
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Gds;
    using Contracts.Gds.Printer;
    using log4net;
    using Protocols;

    /// <summary>
    ///     Manage outgoing messages per the TTL protocol.  Poll for status when no other
    ///     commands are pending.  Process responses.
    /// </summary>
    public class EpicTTLProtocol : SerialPrinter
    {
        private const int TenSecondsMs = 10000;
        private const string None = "None";
        private const int TicketCharactersPerLine = 48;

        /// <summary>Polls status once per the defined MS. the spec requests 100~200ms.</summary>
        private const int PollingIntervalMs = 500;

        /// <summary>How many milliseconds are needed for an expected response</summary>
        private const int ExpectedResponseTime = 250;

        /// <summary>Polls status once per the defined MS. the spec requests 100~200ms.</summary>
        private const int PrintingPollingIntervalMs = 100;

        /// <summary>Estimated milliseconds to complete a set of operations </summary>
        private const int UninterruptableOperationMillisecondsRequired = 20000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Format for VersionResponse message (not free-form bytes with constant length like most)
        ///     [Status1][Status2][Length][Data0][Data1]...
        ///     Where:
        ///     Status1,2 is constant: ENQ '1'
        ///     Length is the total of the variable following data plus itself
        ///     Data0, Data1, ... are additional data bytes of ASCII information
        /// </summary>
        private static readonly MessageTemplate<NullCrcEngine> VersionResponseMessageTemplate =
            new MessageTemplate<NullCrcEngine>(
                new List<MessageTemplateElement>
                {
                    new MessageTemplateElement
                    {
                        ElementType = MessageTemplateElementType.Constant,
                        Length = 2,
                        Value = new[] { EpicTTLProtocolConstants.Enq, (byte)'1' }
                    },
                    new MessageTemplateElement
                    {
                        ElementType = MessageTemplateElementType.LengthPlusDataLength, Length = 1
                    },
                    new MessageTemplateElement
                    {
                        ElementType = MessageTemplateElementType.VariableData, Length = 20
                    }
                },
                0);

        // Map from GDS PDL rotation codes to EpicTTL ones
        private static readonly Dictionary<GdsConstants.PdlRotation, EpicTTLProtocolConstants.EpicTTLPrintDirection>
            RotationMap =
                new Dictionary<GdsConstants.PdlRotation, EpicTTLProtocolConstants.EpicTTLPrintDirection>
                {
                    { GdsConstants.PdlRotation.None, EpicTTLProtocolConstants.EpicTTLPrintDirection.Right },
                    { GdsConstants.PdlRotation.Quarter, EpicTTLProtocolConstants.EpicTTLPrintDirection.Down },
                    { GdsConstants.PdlRotation.Half, EpicTTLProtocolConstants.EpicTTLPrintDirection.Left },
                    { GdsConstants.PdlRotation.ThreeQuarters, EpicTTLProtocolConstants.EpicTTLPrintDirection.Up }
                };

        // Map from GDS PDL justification codes to EpicTTL ones
        private static readonly Dictionary<GdsConstants.PdlJustify, EpicTTLProtocolConstants.EpicTTLPrintJustify>
            JustificationMap =
                new Dictionary<GdsConstants.PdlJustify, EpicTTLProtocolConstants.EpicTTLPrintJustify>
                {
                    { GdsConstants.PdlJustify.Left, EpicTTLProtocolConstants.EpicTTLPrintJustify.Left },
                    { GdsConstants.PdlJustify.Center, EpicTTLProtocolConstants.EpicTTLPrintJustify.Center },
                    { GdsConstants.PdlJustify.Right, EpicTTLProtocolConstants.EpicTTLPrintJustify.Right }
                };

        private readonly Dictionary<int, RegionSaveData> _regionData = new Dictionary<int, RegionSaveData>();
        private readonly Dictionary<int, dprtype> _dprData = new Dictionary<int, dprtype>();
        private readonly Dictionary<int, byte[]> _templateRegions = new Dictionary<int, byte[]>();
        private readonly object _lock = new object();

        private bool _isPrinting;
        private bool _isPrintStarted;
        private bool _isValNumCompleteDetected;
        private bool _selfTestDone;

        /// <summary>
        ///     Construct
        /// </summary>
        public EpicTTLProtocol()
        {
            // A sufficient polling rate
            PollIntervalMs = PollingIntervalMs;

            // The DefaultMessageTemplate (a single VariableData field) is fine for this protocol,
            // normally (except for "Return Version Information" response),
            // because this protocol is so primitive and free-form.

            CommunicationTimeoutMs = 250; // determined by observation, not spec

            MinimumResponseTime = ExpectedResponseTime;

            UseSyncMode = true;
        }

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

        /// <inheritdoc />
        protected override void SelfTest()
        {
            EnablePolling(false);
            RequestStatus();
            ResetToDefault();
            RequestStatus();
            CrcVerification();
            _selfTestDone = true;
            EnablePolling(true);

            OnMessageReceived(new FailureStatus());
        }

        /// <inheritdoc />
        protected override bool GetDeviceInformation()
        {
            DeleteAllRegionsAndTemplates();

            var result = GetFirmwareRevision() && GetVersionInformation();

            // at this point we know the protocol and firmware version
            UpdatePrinterSpecificTemplateMappingsForFirmware();

            return result;
        }

        /// <inheritdoc />
        protected override void CalculateCrc()
        {
            EnablePolling(false);
            CrcVerification();
            EnablePolling(true);
        }

        /// <inheritdoc />
        protected override void Enable(bool enable)
        {
            if (enable)
            {
                ResetToDefault();
            }
            else
            {
                AbortPrint();
            }
        }

        /// <inheritdoc />
        protected override Task<bool> FormFeed()
        {
            var result = SendFormFeed();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        protected override Task<bool> DefineRegion(dprtype region)
        {
            lock (_lock)
            {
                RegionCache.Add(region);
            }

            return Task.FromResult(RenderRegionDefinition(region));
        }

        /// <inheritdoc />
        protected override Task<bool> PrintTicket(PrintCommand ticket, CancellationToken token)
        {
            EnablePolling(false);
            var result = RequestStatus() && RenderPrintData(ticket);
            EnablePolling(true);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        protected override bool RequestStatus()
        {
            lock (_lock)
            {
                var response = AskForPrinterStatus();

                if (response == null)
                {
                    return false;
                }
                // Each status bit corresponds to some standardized fault, warning, or normal state.
                const int statusOffset = 2;

                var status = GenerateStatusFromResponse(response, statusOffset);

                GenerateTicketPrintStatus(status);

                GeneratePrinterStatus(status);

                CheckIfPrintIsCompleted(status);

                CheckIfPrintIsStarted();

                return true;
            }
        }

        private void CheckIfPrintIsStarted()
        {
            _isPrintStarted = _isPrinting && !PrinterStatus.TopOfForm;
        }

        private void CheckIfPrintIsCompleted(EpicTTLProtocolConstants.EpicTTLStatus status)
        {
            if (_isPrinting &&
                status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.NotPrinting) && _isPrintStarted)
            {
                _isPrinting = false;
                TicketPrintStatus = new TicketPrintStatus { PrintComplete = true };
                UpdatePollingRate(PollingIntervalMs);
                ResetToDefault(); // to clear the "completed" bits
            }
        }

        private void GeneratePrinterStatus(EpicTTLProtocolConstants.EpicTTLStatus status)
        {
            if (_selfTestDone)
            {
                PrinterStatus = new PrinterStatus
                {
                    PaperInChute =
                        !_isPrinting &&
                        status.HasFlag(
                            EpicTTLProtocolConstants.EpicTTLStatus
                                .PaperInPath), // Paper In Chute only matters when not printing.
                    PaperLow = status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.PaperLow),
                    ChassisOpen = status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.ChassisIsOpen),
                    PrintHeadOpen = status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.HeadIsUp),
                    PaperJam = status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.PaperJam),
                    PaperEmpty = !_isPrinting &&
                                 !status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.ChassisIsOpen) &&
                                 !status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.HeadIsUp) &&
                                 status.HasFlag(
                                     EpicTTLProtocolConstants.EpicTTLStatus
                                         .OutOfPaper), // Paper Out only matters when not printing.
                    TopOfForm = _isPrinting ||
                                status.HasFlag(
                                    EpicTTLProtocolConstants.EpicTTLStatus
                                        .TopOfForm) || // Top-of-form only matters at certain times.
                                status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.OutOfPaper) ||
                                !status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.NotPrinting)
                };

                if (HasDisablingFault)
                {
                    _isPrinting = false;
                }
            }
        }

        private void GenerateTicketPrintStatus(EpicTTLProtocolConstants.EpicTTLStatus status)
        {
            if (_isPrinting &&
                !_isValNumCompleteDetected &&
                status.HasFlag(EpicTTLProtocolConstants.EpicTTLStatus.ValidationCompleted))
            {
                _isValNumCompleteDetected = true;
                TicketPrintStatus = new TicketPrintStatus { FieldOfInterest1 = true, PrintInProgress = _isPrinting };
            }
        }

        private EpicTTLProtocolConstants.EpicTTLStatus GenerateStatusFromResponse(byte[] response, int statusOffset)
        {
            var status = (EpicTTLProtocolConstants.EpicTTLStatus)ConvertEndianBytesToInt(response, 2, true, statusOffset);
            return status;
        }

        private byte[] AskForPrinterStatus()
        {
            Logger.Debug("send RequestStatus");
            var response = SendMessage(
                EpicTTLProtocolConstants.CommandRequestCombinedPrinterStatus,
                EpicTTLProtocolConstants.CommandRequestCombinedPrinterStatus,
                EpicTTLProtocolConstants.CommandRequestCombinedPrinterStatus.Length + 2);
            return response;
        }

        protected override void SpecialCaseCharacterReplacement(ref string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                // For Ithaca Europe firmware, we must substitute '€' (0x80) with 'Õ' (0xD5), in order for the firmware to print with '€'
                line = line.Replace('€', 'Õ');

                // For Ithaca Philippines firmware, we must substitute '₱' (0x80) with 'PHP ' (substituting '¹' will also cause 'PHP' to be printed)
                line = line.Replace("₱", "PHP ");

                // For Ithaca Korea firmware, we must substitute '₩' with $, in order for the firmware to print with '₩'
                line = line.Replace("₩", "$ ");
            }
        }

        /* Replace the not supported symbols with the empty space otherwise it will print some garbage character on ticket */
        protected override void NotSupportedCharacterReplacement(ref string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            string[] notSupportedCurrencySymbols = { "₱", "₩" };
            foreach (var currencySymbol in notSupportedCurrencySymbols)
            {
                line = line.Replace(currencySymbol, "");
            }
        }

        protected bool ResetToDefault()
        {
            lock (_lock)
            {
                Logger.Debug("send ResetToDefault");
                SendMessage(EpicTTLProtocolConstants.CommandResetToDefaults, EpicTTLProtocolConstants.ResponseEmpty, 0);

                // there is no response so we can only presume it worked
                return true;
            }
        }

        protected bool AbortPrint()
        {
            lock (_lock)
            {
                if (_isPrinting)
                {
                    Logger.Debug("send ResetPrinter");
                    _isPrinting = false;

                    SendMessage(EpicTTLProtocolConstants.CommandAbortPrint, EpicTTLProtocolConstants.ResponseEmpty, 0);
                    UpdatePollingRate(PollingIntervalMs);

                    // This is resetting the device so we need to ignore polling while we are resetting.
                    DisablePolling(EpicTTLProtocolConstants.ResetTimeMs);
                    return true;
                }

                return false;
            }
        }

        private static void CalculateDimensions(ref RegionSaveData regionData, dprtype region)
        {
            // The EpicTTL looks at coordinates differently, depending upon the print orientation.  Using the far edges
            // of the ticket (as known by its dimensions in dots) as starting points is proper for certain dimensions
            // in certain orientations.  None of this is described in any manuals, of course; learned only by process
            // of discovery.
            var breadth = region.dx;
            var potentialOutsideOrigin = EpicTTLProtocolConstants.PaperWidthDots - (region.x + region.dx);
            if (potentialOutsideOrigin < 0)
            {
                breadth += potentialOutsideOrigin * 2;
                potentialOutsideOrigin = 0;
            }

            var length = region.dy;
            var potentialBottomOrigin = EpicTTLProtocolConstants.PaperLengthDots -
                                        EpicTTLProtocolConstants.LandscapeBuffer - (region.y + region.dy);
            if (potentialBottomOrigin < 0)
            {
                length += potentialBottomOrigin * 2;
                potentialBottomOrigin = 0;
            }

            switch (regionData.Direction)
            {
                case EpicTTLProtocolConstants.EpicTTLPrintDirection.Right:
                    regionData.Origin = new Point(region.x, region.y);
                    regionData.Size = new Size(breadth, length);
                    break;
                case EpicTTLProtocolConstants.EpicTTLPrintDirection.Down:
                    regionData.Origin = new Point(
                        region.y,
                        potentialOutsideOrigin - EpicTTLProtocolConstants.LandscapeBuffer);
                    regionData.Size = new Size(length, breadth);
                    break;
                case EpicTTLProtocolConstants.EpicTTLPrintDirection.Left:
                    regionData.Origin = new Point(potentialOutsideOrigin, potentialBottomOrigin);
                    regionData.Size = new Size(breadth, length);
                    break;
                case EpicTTLProtocolConstants.EpicTTLPrintDirection.Up:
                    regionData.Origin = new Point(
                        potentialBottomOrigin,
                        region.x + EpicTTLProtocolConstants.LandscapeBuffer);
                    regionData.Size = new Size(length, breadth);
                    break;
            }
        }

        private byte[] SendMessage(
            byte[] data,
            byte[] responseStart,
            int responseLength,
            IMessageTemplate tempSendTemplate = null,
            IMessageTemplate tempGetTemplate = null)
        {
            byte[] response = null;
            var originalTaskPriority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            try
            {
                // Some commands don't get a response
                if (responseLength == 0)
                {
                    SendCommand(data, tempSendTemplate);
                    WaitSendComplete();
                }
                else
                {
                    response = SendCommandAndGetResponse(data, responseLength, tempSendTemplate, tempGetTemplate);

                    // Commands that get a response, sometimes expect certain exact bytes at the start of the response
                    if (!MatchResponseStart(response, responseStart))
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

        private bool GetVersionInformation()
        {
            const int responseWordLength = 3;
            const int manufacturerWordOffset = 0;
            const int modelWordOffset = 1;
            const int versionWordOffset = 2;

            // Return Version Information (this is the only command whose response has variable length)
            Logger.Debug("send ReturnVersionInformation");
            lock (_lock)
            {
                var response = SendMessage(
                    EpicTTLProtocolConstants.CommandReturnVersionInfo,
                    EpicTTLProtocolConstants.ResponseEmpty,
                    -1,
                    null,
                    VersionResponseMessageTemplate);
                if (response == null)
                {
                    return false;
                }

                // Expected format (ASCII): "Manufacturer Model Version"
                var strResp = Encoding.ASCII.GetString(response);
                var parts = strResp.Split(' ');
                if (parts.Length != responseWordLength)
                {
                    return false;
                }

                Manufacturer = parts[manufacturerWordOffset];
                Model = parts[modelWordOffset];
                FirmwareVersion = parts[versionWordOffset];
                Firmware = FirmwareRevision;
                BootVersion = None;
                return true;
            }
        }

        private bool GetFirmwareRevision()
        {
            const int responseLength = 2;

            Logger.Debug("send ReturnFirmwareRevision");
            lock (_lock)
            {
                var response = SendMessage(
                    EpicTTLProtocolConstants.CommandReturnFirmwareRevision,
                    EpicTTLProtocolConstants.ResponseEmpty,
                    2);
                if (response == null || response.Length != responseLength)
                {
                    return false;
                }

                // Expected response (2 bytes, ASCII): "Revision"
                FirmwareRevision = Encoding.ASCII.GetString(response);
                return true;
            }
        }

        private void CrcVerification()
        {
            const int responseLength = 4;
            const int crcOffset = 2;
            const int crcSize = 2;

            if (FirmwareCrc != UnknownCrc)
            {
                return;
            }

            // This operation can take over five seconds.
            lock (_lock)
            {
                MinimumResponseTime = UninterruptableOperationMillisecondsRequired;

                Logger.Debug("send CrcVerification");

                var response = SendMessage(
                    EpicTTLProtocolConstants.CommandCrcVerification,
                    EpicTTLProtocolConstants.ResponseCrcVerification,
                    EpicTTLProtocolConstants.ResponseCrcVerification.Length + 2);
                if (response != null && response.Length == responseLength)
                {
                    // Expected response: first two bytes mimic the command, last two byte contain the CRC, high byte first
                    FirmwareCrc = ConvertEndianBytesToInt(response, crcSize, true, crcOffset);

                    OnMessageReceived(new CrcData { Result = FirmwareCrc });
                }

                MinimumResponseTime = ExpectedResponseTime;
            }
        }

        private bool SendFormFeed()
        {
            Logger.Debug("send FormFeed");
            lock (_lock)
            {
                _isPrinting = true;
                SendMessage(new[] { EpicTTLProtocolConstants.Ff }, EpicTTLProtocolConstants.ResponseEmpty, 0);
            }

            // there is no response so we can only presume it worked
            return true;
        }

        private bool MatchResponseStart(byte[] response, byte[] responseStart)
        {
            return response != null &&
                   responseStart != null &&
                   responseStart.Length <= response.Length &&
                   CompareByteArrays(response, responseStart, responseStart.Length) == 0;
        }

        protected override bool RenderRegionDefinition(dprtype region)
        {
            lock (_lock)
            {
                var id = byte.Parse(region.id);
                if (_regionData.ContainsKey(id))
                {
                    return true;
                }

                var typeParts = region.type.Split('=');
                var typeCode = (GdsConstants.PdlRegionType)typeParts[0][0];
                var gdsFontNum = int.Parse(typeParts[1]);
                var font = GetDeviceScaledFont(gdsFontNum, EpicTTLProtocolConstants.FontScaleFactors);

                var gdsRotation = (GdsConstants.PdlRotation)int.Parse(region.rot);
                var rotate = RotationMap[gdsRotation];

                var gdsJustify = (GdsConstants.PdlJustify)region.jst;
                var justify = JustificationMap[gdsJustify];

                var regionData = new RegionSaveData
                {
                    RegionCommand = new List<byte>(),
                    LineHeightDots = font.HeightPts * EpicTTLProtocolConstants.DotsPerPoint,
                    Direction = rotate,
                    Justification = justify,
                    IsBarcode = typeCode == GdsConstants.PdlRegionType.Barcode
                };

                CalculateDimensions(ref regionData, region);

                switch (typeCode)
                {
                    case GdsConstants.PdlRegionType.Font:
                        SetTextRegionCommand(regionData, region, font);
                        break;
                    case GdsConstants.PdlRegionType.Barcode:
                        SetBarcodeRegionCommand(regionData, region);
                        break;
                }

                _regionData[id] = regionData;
                _dprData[id] = region;
                return true;
            }
        }

        private void SetTextRegionCommand(RegionSaveData regionData, dprtype region, FontDefinition font)
        {
            const int charCodeMask = 0x07;
            const int charHeightShift = 4;
            const int fontHeightOffset = 2;

            SetPrintDirectionCommand(regionData);

            // Font
            var cmd = (byte[])EpicTTLProtocolConstants.CommandSelectFonts.Clone();
            cmd[fontHeightOffset] = (byte)font.HeightPts;
            regionData.RegionCommand.AddRange(cmd);

            // Char Size (multipliers)
            cmd = (byte[])EpicTTLProtocolConstants.CommandSelectCharacterSize.Clone();
            var widthCode = (region.m1 & charCodeMask) - 1;
            var heightCode = (region.m2 & charCodeMask) - 1;
            cmd[fontHeightOffset] = (byte)(widthCode | (heightCode << charHeightShift));
            regionData.RegionCommand.AddRange(cmd);

            // Double-strike on, but not in portrait mode (becomes too fat)
            if (regionData.Direction == EpicTTLProtocolConstants.EpicTTLPrintDirection.Down
                || regionData.Direction == EpicTTLProtocolConstants.EpicTTLPrintDirection.Up)
            {
                regionData.RegionCommand.AddRange(EpicTTLProtocolConstants.CommandDoubleStrikeOn);
            }
            else
            {
                regionData.RegionCommand.AddRange(EpicTTLProtocolConstants.CommandDoubleStrikeOff);
            }
        }

        private void SetBarcodeRegionCommand(RegionSaveData regionData, dprtype region)
        {
            const int barNarrowOffset = 2;
            const int barWideOffset = 3;
            const int positionOffset = 2;

            SetPrintDirectionCommand(regionData);

            // Double-strike off
            regionData.RegionCommand.AddRange(EpicTTLProtocolConstants.CommandDoubleStrikeOff);

            // Y position
            var cmd = (byte[])EpicTTLProtocolConstants.CommandAbsoluteVerticalPosition.Clone();
            WriteIntToEndianBytes(regionData.Origin.Y, 2, true, cmd, positionOffset);
            regionData.RegionCommand.AddRange(cmd);

            // Barcode start position
            cmd = (byte[])EpicTTLProtocolConstants.CommandBarCodeStartPosition.Clone();
            WriteIntToEndianBytes(regionData.Origin.X, 2, true, cmd, positionOffset);
            regionData.RegionCommand.AddRange(cmd);

            // Barcode element thicknesses
            cmd = (byte[])EpicTTLProtocolConstants.CommandBarCodeElementWidth.Clone();
            cmd[barNarrowOffset] = (byte)region.m1;
            cmd[barWideOffset] = (byte)region.m2;
            regionData.RegionCommand.AddRange(cmd);

            // Barcode height
            cmd = (byte[])EpicTTLProtocolConstants.CommandBarCodeHeight.Clone();
            cmd[positionOffset] = byte.Parse(region.attr);
            regionData.RegionCommand.AddRange(cmd);
        }

        private void SetPrintDirectionCommand(RegionSaveData regionData)
        {
            const int directionOffset = 2;

            var cmd = (byte[])EpicTTLProtocolConstants.CommandPrintDirection.Clone();
            cmd[directionOffset] = (byte)regionData.Direction;
            regionData.RegionCommand.AddRange(cmd);
        }

        private void SetJustifiedBarcodeRegionCommand(RegionSaveData regionData, dprtype region, string barcodeData)
        {
            const int controlCharacters = 2; // I2of5 has 2 control characters
            const int defaultBarcodeWidth = EpicTTLProtocolConstants.DefaultBarcodeLength + controlCharacters;

            regionData.DefaultX = regionData.DefaultX ?? regionData.Origin.X;

            if (regionData.Justification != EpicTTLProtocolConstants.EpicTTLPrintJustify.Center)
            {
                return;
            }

            var width = regionData.Size.Width;
            var barcodeLength = barcodeData.Length + controlCharacters;
            var xShift = (width - width / defaultBarcodeWidth * barcodeLength) / 2;
            var modifiedX = (int)regionData.DefaultX + xShift;

            if (modifiedX == regionData.ModifiedX)
            {
                return;
            }

            regionData.Origin.X = modifiedX;
            regionData.RegionCommand.Clear();
            SetBarcodeRegionCommand(regionData, region);
            regionData.ModifiedX = regionData.Origin.X;
            _regionData[int.Parse(region.id)] = regionData;
        }

        protected override bool RenderTemplateDefinition(dpttype template)
        {
            lock (_lock)
            {
                if (_templateRegions.ContainsKey(template.id))
                {
                    return true;
                }

                var regionIdList =
                    new List<string>(template.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                var regionIds = regionIdList.Select(byte.Parse).ToArray();
                _templateRegions[template.id] = regionIds;

                return true;
            }
        }

        private bool RenderPrintData(PrintCommand ticket)
        {
            return UsePrinterDefinedTemplates
                ? RenderPrinterDefinedPrintData(ticket)
                : RenderPlatformDefinedPrintData(ticket);
        }

        private bool RenderPrinterDefinedPrintData(PrintCommand ticket)
        {
            ResetToDefault();

            lock (_lock)
            {
                if (PrinterSpecificTemplateMappings.IsAuditTicket(ticket))
                {
                    return Render3ColumnTicketData(ticket);
                }

                _isPrinting = true;
                _isValNumCompleteDetected = false;
                TicketPrintStatus = new TicketPrintStatus { PrintInProgress = true };

                AdjustWriteTimeout(TenSecondsMs);

                var remappedTicket =
                    PrinterSpecificTemplateMappings.RemapPrintCommand(ticket, TemplateCache, RegionCache);

                var command = CreatePrintCommand(remappedTicket.PrinterTemplateId, remappedTicket.DataFields.ToList());

                var cmdStr = string.Join(" ", command.Select(byteValue => byteValue.ToString("x2")).ToArray());
                Logger.Debug($"Sending TTL command: {cmdStr}");

                SendMessage(command, EpicTTLProtocolConstants.ResponseEmpty, 0);

                AdjustWriteTimeout(CommunicationTimeoutMs);
                UpdatePollingRate(PrintingPollingIntervalMs);
            }

            return true;
        }

        private void AppendTtlSegmentGroup(List<byte> command, string value)
        {
            SpecialCaseCharacterReplacement(ref value);

            command.Add(EpicTTLProtocolConstants.Gs);
            command.Add((byte)'C');
            command.Add(255);
            command.AddRange(value.ToByteArray());
            command.Add(EpicTTLProtocolConstants.Cr);
        }

        private byte[] CreatePrintCommand(string templateId, List<PrintDataField> printData)
        {
            var command = new List<byte> { EpicTTLProtocolConstants.Esc, (byte)'o' };

            // Template number
            command.AddRange(templateId.ToByteArray());

            AppendTtlSegmentGroup(command, "1");

            foreach (var pData in printData)
            {
                AppendTtlSegmentGroup(command, pData.Data);
            }

            command.Add(EpicTTLProtocolConstants.Ff);

            return command.ToArray();
        }

        private bool RenderPlatformDefinedPrintData(PrintCommand ticket)
        {
            const int dataOffset = 2;
            const int justifyOffset = 2;
            const byte validationNumberFlag = 0x80;
            const int xBeginOffset = 3;
            const int xEndOffset = 5;
            const int barcodeLengthOffset = 3;

            lock (_lock)
            {
                if (!_templateRegions.ContainsKey(ticket.Id))
                {
                    Logger.Debug($"RenderTicketData can't find ticket ID {ticket.Id}");
                    return false;
                }

                if (_templateRegions[ticket.Id].Length < ticket.DataFields.Length)
                {
                    Logger.Debug("RenderTicketData not enough region definitions");
                }

                _isPrinting = true;
                _isValNumCompleteDetected = false;
                TicketPrintStatus = new TicketPrintStatus { PrintInProgress = true };

                AdjustWriteTimeout(TenSecondsMs);

                // Render the fields
                var result = new List<byte>();

                // Start the page with a known state
                result.AddRange(EpicTTLProtocolConstants.CommandResetToDefaults);

                // Put into page mode (same as Landscape mode)
                result.AddRange(EpicTTLProtocolConstants.CommandPageMode);

                var regions = _templateRegions[ticket.Id];
                for (var index = 0; index < ticket.DataFields.Length; index++)
                {
                    if (!_regionData.ContainsKey(regions[index]))
                    {
                        Logger.Error($"Cannot find region {regions[index]}");
                        throw new InvalidOperationException($"Cannot find region {regions[index]}");
                    }

                    var region = _regionData[regions[index]];

                    if (region.IsBarcode)
                    {
                        SetJustifiedBarcodeRegionCommand(
                            region,
                            _dprData[regions[index]],
                            ticket.DataFields[index].Data ?? string.Empty);
                    }

                    result.AddRange(region.RegionCommand.ToArray());

                    // Multi-line text has to be broken up into lines; each line is a "field".
                    // Empty fields were parsed into nulls rather than empty strings.
                    var lines = ticket.DataFields[index].Data?.Split((char)EpicTTLProtocolConstants.Nl) ??
                                new[] { string.Empty };
                    for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                    {
                        var line = lines[lineIndex];
                        if (line.Length <= 0)
                        {
                            continue;
                        }

                        // Specify y precisely
                        if (!region.IsBarcode)
                        {
                            var yPos = region.Origin.Y + lineIndex * region.LineHeightDots;
                            var cmd = (byte[])EpicTTLProtocolConstants.CommandAbsoluteVerticalPosition.Clone();
                            WriteIntToEndianBytes((int)yPos, 2, true, cmd, dataOffset);
                            result.AddRange(cmd);

                            // Set field justification, start and end positions
                            // This is a per-line command, must immediately precede the data
                            cmd = (byte[])EpicTTLProtocolConstants.CommandFieldJustify.Clone();
                            cmd[justifyOffset] = (byte)region.Justification;
                            if (ticket.DataFields[index].IsRegionOfInterest > 0)
                            {
                                cmd[justifyOffset] |= validationNumberFlag;
                            }

                            WriteIntToEndianBytes(region.Origin.X, 2, true, cmd, xBeginOffset);
                            WriteIntToEndianBytes(region.Origin.X + region.Size.Width, 2, true, cmd, xEndOffset);
                            result.AddRange(cmd);
                        }
                        else
                        {
                            // Define barcode type
                            // This is a per-line command, must immediately precede the data
                            var cmd = (byte[])EpicTTLProtocolConstants.CommandPrintBarCodeInterleaved2Of5.Clone();
                            cmd[barcodeLengthOffset] = (byte)line.Length;
                            result.AddRange(cmd);
                        }

                        // ... the actual text
                        result.AddRange(Encoding.ASCII.GetBytes(line));

                        // ... and the required new-line to end the "field"
                        if (!region.IsBarcode)
                        {
                            result.Add(EpicTTLProtocolConstants.Nl);
                        }
                    }
                }

                // Finish the page
                result.Add(EpicTTLProtocolConstants.Ff);

                Logger.Debug("send ticket command");
                SendMessage(result.ToArray(), EpicTTLProtocolConstants.ResponseEmpty, 0);

                AdjustWriteTimeout(CommunicationTimeoutMs);
                UpdatePollingRate(PrintingPollingIntervalMs);

                return true;
            }
        }

        private byte[] MakeLine(
            int index,
            IReadOnlyList<string> left,
            IReadOnlyList<string> center,
            IReadOnlyList<string> right)
        {
            var line = new List<byte>();

            line.AddRange(AlignLine(index, TicketCharactersPerLine, left, center, right));
            line.Add(EpicTTLProtocolConstants.Cr);
            line.Add(EpicTTLProtocolConstants.Nl);

            return line.ToArray();
        }

        private bool Render3ColumnTicketData(PrintCommand ticket)
        {
            // Reset to printer defaults
            SendMessage(EpicTTLProtocolConstants.CommandResetToDefaults, EpicTTLProtocolConstants.ResponseEmpty, 0);

            _isPrinting = true;
            _isValNumCompleteDetected = false;
            TicketPrintStatus = new TicketPrintStatus { PrintInProgress = true };

            AdjustWriteTimeout(TenSecondsMs);

            var header = ticket.DataFields[3].Data ?? string.Empty;

            var leftColumn = ticket.DataFields[0].Data;
            NotSupportedCharacterReplacement(ref leftColumn);

            var centerColumn = ticket.DataFields[1].Data;
            NotSupportedCharacterReplacement(ref centerColumn);

            var rightColumn = ticket.DataFields[2].Data;
            NotSupportedCharacterReplacement(ref rightColumn);

            var builder = new List<byte>();

            // add  printer initialize command
            builder.AddRange(EpicTTLProtocolConstants.CommandResetToDefaults);

            // Set smallest font
            builder.Add(EpicTTLProtocolConstants.Esc);
            builder.Add((byte)'S');

            // print the header text center aligned
            string[] empty = { };
            builder.AddRange(MakeLine(0, empty, new[] { header }, empty));

            // add CR LF to move to next line
            builder.Add(EpicTTLProtocolConstants.Cr);

            // split column strings into lines
            var splitLeft = leftColumn is null ? new string[] { } : leftColumn.Split('\n');
            var splitCenter = centerColumn is null ? new string[] { } : centerColumn.Split('\n');
            var splitRight = rightColumn is null ? new string[] { } : rightColumn.Split('\n');

            // determine which column of text has the most entries
            var max = Math.Max(splitLeft.Length, Math.Max(splitRight.Length, splitCenter.Length));

            // loop thru all the column entries and create lines
            for (var i = 0; i < max; i++)
            {
                var line = MakeLine(i, splitLeft, splitCenter, splitRight);
                builder.AddRange(line);
            }

            // send template mode form feed
            builder.Add(EpicTTLProtocolConstants.Ff);

            var cmdStr = string.Join(" ", builder.Select(byteValue => byteValue.ToString("x2")).ToArray());
            Logger.Debug($"Sending TTL command: {cmdStr}");

            SendMessage(builder.ToArray(), EpicTTLProtocolConstants.ResponseEmpty, 0);

            AdjustWriteTimeout(CommunicationTimeoutMs);
            UpdatePollingRate(PrintingPollingIntervalMs);

            return true;
        }

        private void DeleteAllRegionsAndTemplates()
        {
            lock (_lock)
            {
                RegionCache.Clear();
                TemplateCache.Clear();

                if (UsePrinterDefinedTemplates)
                {
                    return;
                }

                SendMessage(EpicTTLProtocolConstants.CommandResetToDefaults, EpicTTLProtocolConstants.ResponseEmpty, 0);
            }
        }

        /// <summary>
        ///     Data we need to use for printing regions
        /// </summary>
        public struct RegionSaveData
        {
            /// <summary>
            ///     This is the set of constant commands that we pre-load into the printer, indexed by region ID.
            /// </summary>
            public List<byte> RegionCommand;

            /// <summary>
            ///     The origin point
            /// </summary>
            public Point Origin;

            /// <summary>
            ///     The dimensions
            /// </summary>
            public Size Size;

            /// <summary>
            ///     We have to calculate our own y-position for each line; this is the spacing.
            /// </summary>
            public float LineHeightDots;

            /// <summary>
            ///     Text direction implies different uses of the coordinates.
            /// </summary>
            public EpicTTLProtocolConstants.EpicTTLPrintDirection Direction;

            /// <summary>
            ///     We need to know barcode or text at print time.
            /// </summary>
            public bool IsBarcode;

            /// <summary>
            ///     Text justification code.
            /// </summary>
            public EpicTTLProtocolConstants.EpicTTLPrintJustify Justification;

            /// <summary>
            ///     Default X value.
            /// </summary>
            public int? DefaultX;

            /// <summary>
            ///     Modified X value.
            /// </summary>
            public int ModifiedX;
        }
    }
}