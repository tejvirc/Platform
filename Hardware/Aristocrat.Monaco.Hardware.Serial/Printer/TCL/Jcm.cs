namespace Aristocrat.Monaco.Hardware.Serial.Printer.TCL
{
    using System.Collections.Generic;
    using System.Drawing;

    public class Jcm : TclProtocol
    {
        private const string ManufacturerName = "JCM/FutureLogic";
        private const string FutureLogicGen2Tcl = "FutureLogic Gen2 Serial TCL";
        private const string SingaporeGen2Firmware = "GURSNGAA0";

        private const int RegionWidthDots = 512;
        private const int RegionLengthDots = 1100;
        private const int WidthBufferDots = 25;
        private const int LengthBufferDots = 35;

        private const int Gen2MinimumLinesPerTicket = 45;
        private const int Gen5MinimumLinesPerTicket = 36;

        ///  <inheritdoc />
        protected override int TemplateWidthDots => RegionWidthDots;

        ///  <inheritdoc />
        protected override int TemplateLengthDots => RegionLengthDots;

        ///  <inheritdoc />
        protected override void CalculateRegionDimensions(ref TclRegionData regionData, dprtype region)
        {
            // This is a copy of the Nanoptix CalculateRegionDimensions.  We do not currently have a JCM firmware that would need this.
            // The Nanoptix version should be very close but tweaks could be needed specific to JCM.

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
            return gdsFontNumber.ToString();
        }

        ///  <inheritdoc />
        protected override int ParseCrcResponse(byte[] response)
        {
            if (response != null && response.Length == TclProtocolConstants.CrcResponseLength)
            {
                // Expected response: first 3 bytes are message formatting,
                // next 2 bytes contain the CRC with the high byte second,
                // last 2 bytes are message formatting
                return ConvertEndianBytesToInt(
                    response,
                    TclProtocolConstants.CrcSize,
                    false,
                    TclProtocolConstants.CrcOffset);
            }

            return UnknownCrc;
        }

        ///  <inheritdoc />
        protected override List<PrintDataField> PreRenderPrintRegions(int pdlTemplateId, List<PrintDataField> printData)
        {
            return printData;
        }

        ///  <inheritdoc />
        protected override List<string> PreRenderTemplateRegions(List<string> regionIds)
        {
            return regionIds;
        }

        protected override void SpecialCaseCharacterReplacement(ref string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                // For JCM Europe firmware, we must substitute '€' (0x80) with '$' (0x24), in order for the firmware to print with '€'
                // Note: for JCM Holland, its required to substitute '€' with 'Õ'. Since only '$' or '€' can be sent, this is handled in the mapping file.
                line = line.Replace('€', '$');
                // For JCM Philippines firmware, we must substitute '₱' (0x80) with '¹' (0xB9), in order for the firmware to print with 'PHP'
                line = line.Replace('₱', '¹');
                // For JCM Korea firmware, we must substitute '₩' with '²' (0xB2), in order for the firmware to print with '₩'
                if (line.StartsWith("₩"))
                {
                    line = line.Replace("₩", "² ");
                    // Remove all commas, the printer will add them
                    line = line.Replace(",", string.Empty);
                }

                // For JCM Europe firmware, firmware will replace $ with € when it is at the beginning of a line but not the end.
                if (line.EndsWith("$"))
                {
                    line = line.Replace("$", string.Empty);
                    line = line.Insert(0, "$");
                }
            }
        }

        /* Replace the not supported symbols with the empty space otherwise it will print some garbage character on ticket */
        protected override void NotSupportedCharacterReplacement(ref string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            string[] notSupportedCurrencySymbols = { "€", "₱", "₩" };
            foreach (var currencySymbol in notSupportedCurrencySymbols)
            {
                line = line.Replace(currencySymbol, "");
            }
        }

        protected override int GetMinimumNoLinesToPrint()
        {
            if (FutureLogicGen2Tcl == Name)
            {
                if (FirmwareVersion == SingaporeGen2Firmware) 
                    return 0; //no need to add extra new lines

                return Gen2MinimumLinesPerTicket;
            }
            else
            {
                return Gen5MinimumLinesPerTicket;
            }
        }
    }
}
