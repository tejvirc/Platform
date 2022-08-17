namespace Aristocrat.Monaco.Hardware.Serial.Printer.TCL
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using log4net;

    public class Nanoptix : TclProtocol
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const string ManufacturerName = "Nanoptix Inc.";

        private const int RegionWidthDots = 520;
        private const int RegionLengthDots = 1100;
        private const int WidthBufferDots = 25;
        private const int LengthBufferDots = 35;
        private const int PayCheck4CharactersPerLine = 60;
        private const int NextGenCharactersPerLine = 48;
        private const string PayCheck4 = "Nanoptix Inc. PayCheck 4 Serial TCL";

        ///  <inheritdoc />
        protected override int TemplateWidthDots => RegionWidthDots;

        ///  <inheritdoc />
        protected override int TemplateLengthDots => RegionLengthDots;

        ///  <inheritdoc />
        protected override void CalculateRegionDimensions(ref TclRegionData regionData, dprtype region)
        {
            // This was discovered through trial an error.  Left and Up do not exactly match the documentation for
            // this vendor and may work differently for other vendors using the same protocol.  Nanoptix calls this
            // protocol NTL but it is very similar (the same?) to TCL by JCM.

            int xOrigin = region.x;
            int yOrigin = region.y;
            int xSize = region.dx;
            int ySize = region.dy;

            switch (regionData.Direction)
            {
                case TclProtocolConstants.TclPrintDirection.Right:
                    xOrigin = region.x + WidthBufferDots;
                    xSize = xOrigin + xSize > RegionWidthDots
                        ? RegionWidthDots - xOrigin
                        : xSize;
                    ySize = yOrigin + ySize > RegionLengthDots
                        ? RegionLengthDots - yOrigin
                        : ySize;

                    break;
                case TclProtocolConstants.TclPrintDirection.Down:
                    xOrigin = region.x + WidthBufferDots;
                    yOrigin = region.y + LengthBufferDots;

                    break;
                case TclProtocolConstants.TclPrintDirection.Left:
                    xOrigin = region.x + WidthBufferDots > RegionWidthDots
                        ? xOrigin
                        : region.x + WidthBufferDots;
                    yOrigin = region.y + ySize + LengthBufferDots > RegionLengthDots
                        ? yOrigin
                        : region.y + ySize + LengthBufferDots;

                    xOrigin += xSize;
                    if (xOrigin > RegionWidthDots)
                    {
                        xSize = xOrigin - RegionWidthDots;
                        xOrigin = RegionWidthDots;
                    }

                    break;
                case TclProtocolConstants.TclPrintDirection.Up:
                    xOrigin = region.x + WidthBufferDots > RegionWidthDots
                        ? xOrigin
                        : region.x + WidthBufferDots;
                    yOrigin = region.y + ySize > RegionLengthDots
                        ? yOrigin
                        : region.y + ySize;

                    xOrigin += xSize;
                    if (xOrigin > RegionWidthDots)
                    {
                        xSize = xOrigin - RegionWidthDots;
                        xOrigin = RegionWidthDots;
                    }

                    break;
            }

            regionData.Origin = new Point(xOrigin, yOrigin);
            regionData.Size = new Size(xSize, ySize);
        }

        ///  <inheritdoc />
        protected override bool GetDeviceSpecificInformation()
        {
            if (!RequestStatus())
            {
                // Firmware version comes from status
                return false;
            }

            Manufacturer = ManufacturerName;

            return true;
        }

        ///  <inheritdoc />
        protected override string GetMappedFont(int gdsFontNumber)
        {
            switch (gdsFontNumber)
            {
                case 2:
                case 3:
                    return gdsFontNumber.ToString();
                case 6:
                case 8:
                    return "7";
                default:
                    return "6";
            }
        }

        ///  <inheritdoc />
        protected override int ParseCrcResponse(byte[] response)
        {
            if (response != null && response.Length == TclProtocolConstants.CrcResponseLength)
            {
                // Expected response: first 3 bytes are message formatting,
                // next 2 bytes contain the CRC with the high byte first,
                // last 2 bytes are message formatting
                return ConvertEndianBytesToInt(
                    response,
                    TclProtocolConstants.CrcSize,
                    true,
                    TclProtocolConstants.CrcOffset);
            }

            return UnknownCrc;
        }

        ///  <inheritdoc />
        protected override List<PrintDataField> PreRenderPrintRegions(int pdlTemplateId, List<PrintDataField> printData)
        {
            var reorderedFields = printData.Where(
                x =>
                    !TemplateBarcodeRegionIndexes[pdlTemplateId].Contains(printData.IndexOf(x))).ToList();

            reorderedFields.AddRange(
                printData.Where(
                    x =>
                        TemplateBarcodeRegionIndexes[pdlTemplateId].Contains(printData.IndexOf(x))).ToList()
            );

            return reorderedFields;
        }

        ///  <inheritdoc />
        protected override List<string> PreRenderTemplateRegions(List<string> regionIds)
        {
            // Not documented but Nanoptix NextGen appears to need the barcode to be the last region or it won't print.
            List<string> reorderedRegionIds = regionIds.Where(x => !BarcodeRegionIds.Contains(x)).ToList();
            reorderedRegionIds.AddRange(regionIds.Where(x => BarcodeRegionIds.Contains(x)).ToList());

            return reorderedRegionIds;
        }

        protected override bool Render3ColumnTicketData(PrintCommand ticket)
        {
            Logger.Debug("Rendering 3 column ticket in line mode");
            string[] header = { ticket.DataFields[3].Data ?? string.Empty };

            var leftColumn = ticket.DataFields[0].Data;
            SpecialCaseCharacterReplacement(ref leftColumn);

            var centerColumn = ticket.DataFields[1].Data;
            SpecialCaseCharacterReplacement(ref centerColumn);

            var rightColumn = ticket.DataFields[2].Data;
            SpecialCaseCharacterReplacement(ref rightColumn);

            var builder = new List<byte>();
            // send printer initialize command
            builder.AddRange(TclProtocolConstants.CommandInitialize);

            //To control the space b/w the lines , this command doesn't work for other Nanoptix firmwares
            if (Name == PayCheck4)
            {
                builder.AddRange(TclProtocolConstants.CommandInitializePageMode);
            }

            // send unit of measure 1/180"
            builder.AddRange(TclProtocolConstants.CommandSetUnitOfMeasure);
            // set 20.5 cpi font
            builder.AddRange(TclProtocolConstants.CommandSet20_5CharactersPerInch);
            // set 1/8" line spacing
            builder.AddRange(TclProtocolConstants.CommandSetLineSpacing);
            // set bold font
            //  builder.AddRange(TclProtocolConstants.CommandSetBoldFont);

            // print the header text center aligned
            builder.AddRange(MakeLine(0, Array.Empty<string>(), header, Array.Empty<string>()));

            // add CR LF to move to next line
            builder.AddRange(TclProtocolConstants.CrLf);

            // split column strings into lines
            var splitLeft = leftColumn is null ? new string[] { } : leftColumn.Split('\n');
            var splitCenter = centerColumn is null ? new string[] { } : centerColumn.Split('\n');
            var splitRight = rightColumn is null ? new string[] { } : rightColumn.Split('\n');

            // determine which column of text has the most entries
            var max = Math.Max(splitLeft.Length, Math.Max(splitRight.Length, splitCenter.Length));

            // loop thru all the column entries and create lines
            for (var i = 0; i < max; i++)
            {
                builder.AddRange(MakeLine(i, splitLeft, splitCenter, splitRight));
            }

            builder.AddRange(TclProtocolConstants.CrLf);

            // send line mode form feed
            builder.Add(TclProtocolConstants.Ff);

            Logger.Debug($"sending audit ticket print message '{BitConverter.ToString(builder.ToArray())}'");
            SendPrintMessage(builder.ToArray(), false);

            // switch back to template mode so ticket ejects fully
            SendMessage(TclProtocolConstants.EnableTemplatePrintingModeCommand, TclProtocolConstants.EmptyResponseLength);
            return true;
        }

        private byte[] MakeLine(int index, IReadOnlyList<string> left, IReadOnlyList<string> center, IReadOnlyList<string> right)
        {
            var line = new List<byte>();
            var ticketCharactersPerLine = Name == PayCheck4 ? PayCheck4CharactersPerLine : NextGenCharactersPerLine;
            line.AddRange(AlignLine(index, ticketCharactersPerLine, left, center, right));
            line.AddRange(TclProtocolConstants.CrLf);

            return line.ToArray();
        }

        ///  <inheritdoc />
        protected override void SpecialCaseCharacterReplacement(ref string line)
        {
            if (!string.IsNullOrEmpty(line))
                line = line.Replace('€', 'Õ');
        }

        /* Replace the not supported symbols with the empty space otherwise it will print some garbage characters on ticket */
        protected override void NotSupportedCharacterReplacement(ref string line)
        {
        }
    }
}
