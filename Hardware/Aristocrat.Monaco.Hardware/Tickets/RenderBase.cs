namespace Aristocrat.Monaco.Hardware.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts.TicketContent;
    using log4net;

    /// <summary>
    ///     Definition of the RenderBase class.
    ///     It has the implementation for the common features (Organizing the text based on the printing mode.)
    ///     in the rendering mechanism of the tickets across the printers.
    /// </summary>
    [Obsolete]
    public abstract class RenderBase : IRender
    {
        /// <summary>Constant identifiers for attribute types.</summary>
        public enum AttributeIdentifier
        {
            /// <summary>No identifier set.</summary>
            None = 0,

            /// <summary>Cash attribute identifier.</summary>
            Cash = 102,

            /// <summary>Establishment attribute identifier.</summary>
            Establishment = 103,

            /// <summary>Validation attribute identifier.</summary>
            Validation = 101,

            /// <summary>Cashout Validation attribute identifier.</summary>
            CashoutValidation = 104,

            /// <summary>Barcode validation attribute identifier.</summary>
            BarcodeValidation = 105,

            /// <summary>Cashout title attribute identifier.</summary>
            CashoutTitle = 106,

            /// <summary>Alpha cash attribute identifier.</summary>
            AlphaCash = 107,

            /// <summary>Keep as is identifier.</summary>
            KeepAsIs = 108,

            /// <summary>Ticket Void After attribute identifier.</summary>
            TicketVoidAfter = 109,

            /// <summary>Establishment Address attribute identifier.</summary>
            EstablishmentAddress = 110,

            /// <summary>Signature line attribute identifier.</summary>
            SignatureLine = 111
        }

        /// <summary>ASCII control characters for printer command.</summary>
        public enum CommandPrimitive
        {
            /// <summary>No command primitive set.</summary>
            None = 0,

            /// <summary>Escape character</summary>
            Escape = 0x1b,

            /// <summary>Line feed character</summary>
            Linefeed = 0x0a,

            /// <summary>Carriage return character</summary>
            CarriageReturn = 0x0d,

            /// <summary>Form feed character</summary>
            FormFeed = 0x0c,

            /// <summary>Group separator character</summary>
            GroupSeparator = 0x1d,

            /// <summary>Shift out character</summary>
            ShiftOut = 0x0e,

            /// <summary>Horizontal tab character</summary>
            HorizontalTab = 0x09,

            /// <summary>Device control 1 character</summary>
            DeviceControl1 = 0x11,

            /// <summary>Device control 2 character</summary>
            DeviceControl2 = 0x12,

            /// <summary>Device control 3 character</summary>
            DeviceControl3 = 0x13,

            /// <summary>Device control 4 character</summary>
            DeviceControl4 = 0x14
        }

        /// <summary>Barcode height identifier.</summary>
        public const int BarcodeHeight = 100;

        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>Initializes a new instance of the <see cref="RenderBase" /> class.</summary>
        protected RenderBase()
        {
            TicketType = "TICKET_TYPE_INVALID";
            SupportedFonts = 0;
            LineSpace = 6;
            LastCharWideMode = CharSizeMode.Normal;
            LastCharHighMode = CharSizeMode.Normal;
            LastFontId = -1;
            LastOrientation = PrintOrientation.Portrait;
            TemplateId = 0;
            DotsUsedInPortrait = 0;

            Commands = new List<char>();
        }

        /// <summary>Gets or sets commands used for the member functions.</summary>
        public List<char> Commands { get; set; }

        /// <summary>Gets or sets the ID of print template.</summary>
        public int TemplateId { get; set; }

        /// <summary>Gets or sets The ticket type being rendered.</summary>
        public string TicketType { get; set; }

        /// <summary>Gets or sets Number of fonts supported.</summary>
        public int SupportedFonts { get; set; }

        /// <summary>Gets or sets The last character width.</summary>
        public CharSizeMode LastCharWideMode { get; set; }

        /// <summary>Gets or sets The last character height.</summary>
        public CharSizeMode LastCharHighMode { get; set; }

        /// <summary>Gets or sets The last font id.</summary>
        public int LastFontId { get; set; }

        /// <summary>Gets or sets The last print orientation.</summary>
        public PrintOrientation LastOrientation { get; set; }

        /// <summary>Gets or sets The number of dots consumed in portrait mode.</summary>
        public int DotsUsedInPortrait { get; set; }

        /// <summary>Gets or sets Dots between two lines.</summary>
        public int LineSpace { get; set; }

        /// <summary>Gets RawData.</summary>
        public string RawData
        {
            get
            {
                var builder = new StringBuilder();

                foreach (var command in Commands)
                {
                    builder.Append(command);
                }

                return builder.ToString().Trim(' ');
            }
        }

        /// <summary>Render multiple pages.</summary>
        /// <param name="pages">A list of pages.</param>
        /// <returns>True if successful.</returns>
        public bool RenderPages(IList<IList<PrintableRegion>> pages)
        {
            if (pages.Count <= 0)
            {
                Logger.Warn("No pages to render.");
                return false;
            }

            // Empty previous stored commands.
            Commands.Clear();

            ResetPrinter();
            BeginTicket();

            foreach (var page in pages)
            {
                if (!Render(page))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Render one page.</summary>
        /// <param name="page">The page to render.</param>
        /// <returns>True if successful.</returns>
        public bool Render(IList<PrintableRegion> page)
        {
            var organizedPrintableRegions = Reorganize(page as List<PrintableRegion>);
            var regionType = new RegionType { PrintType = PrintType.Unknown, TypeValue = string.Empty };
            var status = false;

            foreach (var region in organizedPrintableRegions)
            {
                regionType = TranslateType(region.Format);

                switch (regionType.PrintType)
                {
                    case PrintType.Font:
                        try
                        {
                            RenderText(region, int.Parse(regionType.TypeValue, CultureInfo.InvariantCulture));
                            status = true;
                        }
                        catch (InvalidTicketConfigurationException)
                        {
                            Logger.Warn("Invalid ticket configuration for PrintType.Font.");
                            status = false;
                        }

                        break;
                    case PrintType.Graphics:
                        status = RenderGraphic(region);
                        break;
                    case PrintType.Barcode:
                        status = RenderBarcode(
                            region,
                            int.Parse(regionType.TypeValue, CultureInfo.InvariantCulture));
                        break;
                    default:
                        Logger.WarnFormat("Unhandled regionType.PrintType {0}", regionType.PrintType);
                        status = false;
                        break;
                }
            }

            if (status)
            {
                AddFormFeed();
            }

            return status;
        }

        /// <summary>Get the region's type and value.</summary>
        /// <param name="type">The source string containing type and value.</param>
        /// <returns>RegionType value</returns>
        public RegionType TranslateType(string type)
        {
            var regionType = new RegionType();

            if (string.IsNullOrEmpty(type) || !type.Contains('='))
            {
                regionType.PrintType = PrintType.Unknown;
                regionType.TypeValue = string.Empty;
                return regionType;
            }

            var first = type.Split('=')[0].Trim();

            switch (first)
            {
                case "F":
                    regionType.PrintType = PrintType.Font;
                    break;
                case "G":
                    regionType.PrintType = PrintType.Graphics;
                    break;
                case "B":
                    regionType.PrintType = PrintType.Barcode;
                    break;
                default:
                    regionType.PrintType = PrintType.Unknown;
                    regionType.TypeValue = string.Empty;
                    return regionType;
            }

            regionType.TypeValue = type.Split('=')[1].Trim();
            return regionType;
        }

        /// <summary>Assign the printing property for the current rendering session.</summary>
        /// <param name="properties">The properties.</param>
        public void SetTicketProperties(Dictionary<string, string> properties)
        {
            TicketType = properties["ticket type"];
        }

        /// <summary>Reorganize the regions.</summary>
        /// <param name="regions">The regions to be reorganized.</param>
        /// <returns>Result of reorganization.</returns>
        public List<PrintableRegion> Reorganize(List<PrintableRegion> regions)
        {
            var portraitRegionsMap = new Dictionary<int, List<PrintableRegion>>();
            var landscapeRegionsMap = new Dictionary<int, List<PrintableRegion>>();

            // Split regions according to the mode (portrait or landscape).
            foreach (var region in regions)
            {
                var rotation = (RegionRotation)region.Rotation;
                var orientation = rotation == RegionRotation.Rotation0 || rotation == RegionRotation.Rotation180
                    ? PrintOrientation.Portrait
                    : PrintOrientation.Landscape;

                if (orientation == PrintOrientation.Portrait)
                {
                    if (portraitRegionsMap.ContainsKey(region.OriginY))
                    {
                        portraitRegionsMap[region.OriginY].Add(region);
                    }
                    else
                    {
                        var tempList = new List<PrintableRegion> { region };
                        portraitRegionsMap[region.OriginY] = tempList;
                    }
                }
                else
                {
                    if (landscapeRegionsMap.ContainsKey(region.OriginX))
                    {
                        landscapeRegionsMap[region.OriginX].Add(region);
                    }
                    else
                    {
                        var tempList = new List<PrintableRegion> { region };
                        landscapeRegionsMap[region.OriginX] = tempList;
                    }
                }
            }

            // Organize the regions in portrait mode.
            var portraitRegions = Reorganize(portraitRegionsMap, PrintOrientation.Portrait);

            var portraitRegionsReference = portraitRegions.ToList();

            // Sort by Origin Y
            portraitRegionsReference.Sort(
                (obj1, obj2) => obj1.OriginY.CompareTo(obj2.OriginY));

            Logger.Debug("Translating the portrait regions:");
            TranslatePosition(PrintOrientation.Portrait, portraitRegionsReference);

            //// Get the last region in portrait to get the y position.
            //// NOTE: The information ticket is an exception. It does not have content in landscape mode.

            DotsUsedInPortrait = 0;
            if (TicketType != "text")
            {
                foreach (var region in portraitRegions)
                {
                    DotsUsedInPortrait += region.SizeY;
                }
            }

            // Organize the regions in landscape mode.
            var landscapeRegions = Reorganize(landscapeRegionsMap, PrintOrientation.Landscape);

            var landscapeRegionsReference = landscapeRegions.ToList();

            landscapeRegionsReference.Sort(
                (obj1, obj2) => obj1.OriginX.CompareTo(obj2.OriginX));

            Logger.Debug("Translating the landscape regions:");
            TranslatePosition(PrintOrientation.Landscape, landscapeRegionsReference);

            // Merge the two lists into result.
            portraitRegionsReference.AddRange(landscapeRegionsReference);

            return portraitRegionsReference;
        }

        /// <summary>Do initialization before producing the command sequence.</summary>
        public abstract void BeginTicket();

        /// <summary>Check if the font index defined in the XML is valid.</summary>
        /// <param name="fontIndex">The font index.</param>
        /// <returns>True if it is valid.</returns>
        public abstract bool IsFontIndexValid(int fontIndex);

        /// <summary>Given dots, calculate how many characters are within the dots.</summary>
        /// <param name="dots">The dots for calculation.</param>
        /// <param name="fontIndex">The font index.</param>
        /// <param name="doublewide">The doublewide.</param>
        /// <returns>The number of characters.</returns>
        public abstract int CalculateCharsInDots(int dots, int fontIndex, bool doublewide);

        /// <summary>Get total dots in each mode.</summary>
        /// <param name="portrait">Indicate if in portrait mode.</param>
        /// <param name="margin">Indicate if margin is used.</param>
        /// <returns>The number of dots.</returns>
        public abstract double GetTotalDots(bool portrait, bool margin);

        /// <summary>Get dots occupied by margin in each mode.</summary>
        /// <param name="portrait">Indicate if in portrait mode.</param>
        /// <returns>The number of dots.</returns>
        public abstract double GetDotsInMargin(bool portrait);

        /// <summary>Align a line.</summary>
        /// <param name="fontIndex">The font index.</param>
        /// <param name="portrait">Indicate if in portrait mode.</param>
        /// <param name="doublewide">The doublewide.</param>
        /// <param name="text">The text to be aligned.</param>
        /// <param name="align">The alignment type.</param>
        /// <param name="totalDots">The total dots available in region.</param>
        /// <param name="region">The region.</param>
        /// <param name="charactersUsed">The number of characters already used on the line.</param>
        /// <returns>The aligned characters.</returns>
        public abstract string AlignTextInRegion(
            int fontIndex,
            bool portrait,
            bool doublewide,
            string text,
            PrintAlignment align,
            int totalDots,
            PrintableRegion region,
            int charactersUsed);

        /// <summary>Translate position in PDL to the position used in printer firmware.</summary>
        /// <param name="mode">The print orientation mode.</param>
        /// <param name="regions">The regions.</param>
        public abstract void TranslatePosition(PrintOrientation mode, IList<PrintableRegion> regions);

        /// <summary>Get font by calculating from available space.</summary>
        /// <param name="fontIndex">The font Index.</param>
        /// <param name="portrait">Indicate if in portrait mode.</param>
        /// <param name="doublewide">Indicate if double wide.</param>
        /// <param name="stringLength">The string length to be printed.</param>
        /// <param name="totalDots">The total dots available.</param>
        public abstract void GetFont(
            ref int fontIndex,
            bool portrait,
            bool doublewide,
            int stringLength,
            int totalDots);

        /// <summary>Select a font for the current region.</summary>
        /// <param name="fontIndex">The font index.</param>
        /// <param name="portrait">Indicate if in portrait mode.</param>
        /// <param name="doublewide">Indicate if double wide.</param>
        /// <param name="stringLength">The string length.</param>
        /// <param name="totalDots">The total dots available.</param>
        public abstract void SelectFont(
            ref int fontIndex,
            bool portrait,
            bool doublewide,
            int stringLength,
            int totalDots);

        /// <summary>Get the smallest font information for printer.</summary>
        /// <param name="index">The font index.</param>
        /// <param name="width">The font width.</param>
        /// <param name="height">The font height.</param>
        public abstract void GetSmallestFontInfo(ref int index, ref int width, ref int height);

        /// <summary>Render a text region.</summary>
        /// <param name="region">The region.</param>
        /// <param name="fontIndex">The font index.</param>
        public abstract void RenderText(PrintableRegion region, int fontIndex);

        /// <summary>Render a barcode region.</summary>
        /// <param name="region">The region.</param>
        /// <param name="barcodeIndex">The barcode index.</param>
        /// <returns>True if successful.</returns>
        public abstract bool RenderBarcode(PrintableRegion region, int barcodeIndex);

        /// <summary>Render a graphic region.</summary>
        /// <param name="region">The region.</param>
        /// <returns>True if successful.</returns>
        public abstract bool RenderGraphic(PrintableRegion region);

        /// <summary>Adds command(s) to change page.</summary>
        public abstract void AddFormFeed();

        /// <summary>The command issued to printer that resets state-related variables.</summary>
        public virtual void ResetPrinter()
        {
            LastCharWideMode = CharSizeMode.Normal;
            LastCharHighMode = CharSizeMode.Normal;
            LastFontId = -1;
            LastOrientation = PrintOrientation.Portrait;
        }

        /// <summary>Insert a blank region with one-line height. Only applied in portrait mode.</summary>
        /// <param name="fontIndex">The font index.</param>
        /// <param name="fontHeight">The font height.</param>
        /// <param name="positionYAxis">The position Y Axis.</param>
        /// <returns>A blank region.</returns>
        private static PrintableRegion MakeBlankLine(int fontIndex, int fontHeight, int positionYAxis)
        {
            return new PrintableRegion(
                "BLANK LINE REGION",
                string.Empty,
                (int)RenderBaseEnumeration.BlankLineDprid,
                0,
                string.Format("{0:D5}", 0),
                positionYAxis,
                string.Format("{0:D5}", positionYAxis),
                100,
                string.Format("{0:D5}", 100),
                fontHeight,
                string.Format("{0:D5}", fontHeight),
                (int)RegionRotation.Rotation0,
                (int)PrintAlignment.Left,
                "F=" + fontIndex,
                (int)CharSizeMode.Normal,
                (int)CharSizeMode.Normal,
                "01",
                " ");
        }

        /// <summary>Count how many lines are in this page.</summary>
        /// <param name="page">The page being rendered.</param>
        /// <returns>The number of non-empty lines.</returns>
        private static int CountPrintableLines(IEnumerable<PrintableRegion> page)
        {
            return page.Count(region => !string.IsNullOrEmpty(region.DefaultText) && region.DefaultText != "\n");
        }

        /// <summary>Reorganize the text regions in the specified mode.</summary>
        /// <param name="regions">The regions to be sorted.</param>
        /// <param name="mode">The print orientation.</param>
        /// <returns>The result of the organization.</returns>
        private IEnumerable<PrintableRegion> Reorganize(
            Dictionary<int, List<PrintableRegion>> regions,
            PrintOrientation mode)
        {
            var regionType = new RegionType { TypeValue = string.Empty, PrintType = PrintType.Unknown };
            PrintableRegion newAttribute = null;
            string temp;
            PrintableRegion attributeDpr;
            var barcodeDpr = new List<PrintableRegion>();
            var destinationPage = new List<PrintableRegion>();

            var leftYInLandscape = (int)GetTotalDots(false, true);

            foreach (var region in regions)
            {
                // Sort the regions in the same line and set the start position.
                int previousPosition;
                if (mode == PrintOrientation.Portrait)
                {
                    region.Value.Sort(
                        (obj1, obj2) => obj1.OriginX.CompareTo(obj2.OriginX));

                    previousPosition = 0;
                }
                else
                {
                    // please notice that landscape is sorted backward from portrait
                    // and also on the y axis rather than the x axis.
                    // things on the same line have the same x value and start at the bottom of the ticket.
                    region.Value.Sort(
                        (obj1, obj2) => obj2.OriginY.CompareTo(obj1.OriginY));

                    previousPosition = (int)RenderBaseEnumeration.LandscapePageWidth;
                }

                var line = string.Empty;
                var fontIndex = SupportedFonts;
                var doubleWidth = false;
                var textRegionCount = 0;

                // Enumerate to form a line.
                foreach (var printableRegion in region.Value)
                {
                    attributeDpr = printableRegion;
                    regionType = TranslateType(attributeDpr.Format);

                    if (regionType.PrintType == PrintType.Barcode)
                    {
                        // Do not organize the barcode region.
                        barcodeDpr.Add(attributeDpr);
                        continue;
                    }

                    // Use first text region as guide. Other regions follow the same font attributes.
                    if (textRegionCount++ == 0)
                    {
                        newAttribute = printableRegion;

                        try
                        {
                            fontIndex = int.Parse(regionType.TypeValue, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException)
                        {
                            Logger.ErrorFormat(
                                "Invalid format for print type in XML: (DPR Id = {0}, Type = {1}",
                                attributeDpr.Id,
                                regionType.TypeValue);
                            continue;
                        }

                        if (!IsFontIndexValid(fontIndex))
                        {
                            // Don't terminate rendering, just skip region & log error.
                            Logger.WarnFormat(
                                "Wrong font index defined in XML: (DPR Id = {0}, Font Index = {1}",
                                attributeDpr.Id,
                                fontIndex);
                            continue;
                        }

                        doubleWidth = newAttribute.Modifier1 >= 2;
                    }

                    // Check if region overlaps previous region.
                    var chars = 0;
                    if (mode == PrintOrientation.Portrait)
                    {
                        if (attributeDpr.OriginX < previousPosition)
                        {
                            chars = CalculateCharsInDots(
                                previousPosition - attributeDpr.OriginX,
                                fontIndex,
                                doubleWidth);
                        }
                    }
                    else
                    {
                        if (attributeDpr.OriginY > previousPosition)
                        {
                            chars = CalculateCharsInDots(
                                attributeDpr.OriginY - previousPosition,
                                fontIndex,
                                doubleWidth);
                        }
                    }

                    // Remove overlapping characters.
                    if (chars != 0)
                    {
                        line = chars >= line.Length ? string.Empty : line.Remove(line.Length - chars);
                    }

                    // Calculate how many characters need to be inserted between two regions.
                    int regionWidth;
                    if (mode == PrintOrientation.Portrait)
                    {
                        chars = CalculateCharsInDots(
                            attributeDpr.OriginX - previousPosition,
                            fontIndex,
                            doubleWidth);

                        regionWidth = attributeDpr.SizeX;

                        // Enforce field legth for anything  other than information data, game history called numbers, and
                        // game history bingo card.
                        if (attributeDpr.Id != 448
                            && attributeDpr.Id != 332
                            && attributeDpr.Id != 334)
                        {
                            // This is a good place to remove characters that exceed the region's boundaries.
                            attributeDpr.DefaultText = EnforceFieldLength(attributeDpr, doubleWidth);
                        }

                        // Align the text in this region.
                        temp = AlignTextInRegion(
                            !attributeDpr.FindAttribute(AttributeIdentifier.Establishment) ? fontIndex : 4,
                            true,
                            doubleWidth,
                            attributeDpr.DefaultText,
                            (PrintAlignment)attributeDpr.Justification,
                            regionWidth,
                            attributeDpr,
                            line.Length);
                    }
                    else
                    {
                        chars = CalculateCharsInDots(
                            previousPosition - attributeDpr.OriginY,
                            fontIndex,
                            doubleWidth);

                        regionWidth = attributeDpr.SizeY;

                        // This is another good place to remove characters that exceed the region's boundaries.
                        attributeDpr.DefaultText = EnforceFieldLength(attributeDpr, doubleWidth);

                        // Align the text in this region.
                        temp = AlignTextInRegion(
                            !attributeDpr.FindAttribute(AttributeIdentifier.Establishment) ? fontIndex : 4,
                            false,
                            doubleWidth,
                            attributeDpr.DefaultText,
                            (PrintAlignment)attributeDpr.Justification,
                            regionWidth,
                            attributeDpr,
                            line.Length);

                        if (chars > temp.Length + 1)
                        {
                            temp = temp.PadLeft(temp.Length + 1);
                        }
                    }

                    line += temp;

                    // Change the previous position
                    if (mode == PrintOrientation.Portrait)
                    {
                        previousPosition = attributeDpr.OriginX + attributeDpr.SizeX;
                    }
                    else
                    {
                        previousPosition = attributeDpr.OriginY + attributeDpr.SizeY;
                    }
                }

                if (textRegionCount > 0)
                {
                    if (mode == PrintOrientation.Landscape ||
                        mode == PrintOrientation.Portrait && textRegionCount == 1 &&
                        newAttribute.Justification == (int)PrintAlignment.Center)
                    {
                        // Remove spaces at the head and tail.
                        newAttribute.DefaultText = line.Trim();
                        newAttribute.Justification = (int)PrintAlignment.Center;
                    }
                    else
                    {
                        newAttribute.DefaultText = line;

                        // Text justification should be left if it's not a single line.
                        if (line.Contains("\n") && textRegionCount > 1)
                        {
                            newAttribute.Justification = (int)PrintAlignment.Left;
                        }
                    }

                    if (mode == PrintOrientation.Portrait)
                    {
                        var previousValue = newAttribute.DefaultText.Trim();

                        var optionalRgn = newAttribute.GetContextValue('9', 3, out var contextValue) &&
                                          (contextValue == 1 || contextValue == 2);

                        if (optionalRgn && string.IsNullOrEmpty(previousValue))
                        {
                            newAttribute.DefaultText = string.Empty;
                        }
                    }

                    // Adjust the x/y axis.
                    if (mode == PrintOrientation.Portrait)
                    {
                        newAttribute.OriginX = 0;
                    }
                    else
                    {
                        newAttribute.OriginY = leftYInLandscape;
                    }

                    // If in portrait mode, check for need to add blank lines before new region.
                    if (mode == PrintOrientation.Portrait)
                    {
                        var lastYPos = 0;
                        int diffY; // Difference in dots.
                        if (destinationPage.Count != 0)
                        {
                            var previousAttribute = destinationPage[destinationPage.Count - 1];
                            lastYPos = previousAttribute.OriginY + previousAttribute.SizeY;
                            diffY = newAttribute.OriginY - lastYPos;
                        }
                        else
                        {
                            diffY = newAttribute.OriginY;
                        }

                        var newFontIndex = 0;
                        var fontWidth = 0;
                        var fontHeight = 0;

                        GetSmallestFontInfo(ref newFontIndex, ref fontWidth, ref fontHeight);

                        for (var i = diffY - fontHeight; i > 0; i -= fontHeight)
                        {
                            destinationPage.Add(MakeBlankLine(newFontIndex, fontHeight, lastYPos));
                            lastYPos += fontHeight;
                        }
                    }

                    // Check for a blank line inserter region.
                    var attribute = newAttribute.Attribute;
                    var isBlankLineInserter = newAttribute.GetContextValue('7', 3, out var newContextValue) ||
                                              newAttribute.GetContextValue('8', 3, out newContextValue);

                    if (mode == PrintOrientation.Portrait && isBlankLineInserter)
                    {
                        int blankLines;
                        if (attribute[0] == '7')
                        {
                            blankLines = newContextValue;
                        }
                        else
                        {
                            var existingLines = CountPrintableLines(destinationPage);
                            blankLines = newContextValue > existingLines
                                ? newContextValue - existingLines
                                : 0;
                        }

                        if (blankLines > 0)
                        {
                            var newFontIndex = 0;
                            var fontWidth = 0;
                            var fontHeight = 0;

                            GetSmallestFontInfo(ref newFontIndex, ref fontWidth, ref fontHeight);
                            var lastYPos = 0;
                            if (destinationPage.Count != 0)
                            {
                                var previousAttribute = destinationPage[destinationPage.Count - 1];
                                lastYPos = previousAttribute.OriginY + previousAttribute.SizeY;
                            }

                            for (var i = 0; i < blankLines; i++)
                            {
                                lastYPos += fontHeight;
                            }
                        }

                        Logger.DebugFormat("end of this pass:{0}", newAttribute.DefaultText);
                    }

                    // Save the new formed text region if not a blank line inserter region.
                    if (!isBlankLineInserter)
                    {
                        Logger.DebugFormat("Final text is:{0}", newAttribute.DefaultText);
                        destinationPage.Add(newAttribute);
                    }
                }
            }

            // Append the barcode region.
            destinationPage.AddRange(barcodeDpr);

            return destinationPage;
        }

        /// <summary>
        ///     Truncate attribute's default text if it exceeds the attribute's size.
        /// </summary>
        /// <param name="attributeDpr">The attribute</param>
        /// <param name="doubleWidth">A boolean.  True if doubleWidth characters in this attribute.</param>
        /// <returns>A possibly revised string for default text.</returns>
        private string EnforceFieldLength(PrintableRegion attributeDpr, bool doubleWidth)
        {
            // This is a good place to remove characters that exceed the region's boundaries.
            var textLength = attributeDpr.DefaultText.Length;
            var fontIndex = SupportedFonts;
            var ydimensionSize = attributeDpr.SizeY;
            var xdimensionSize = attributeDpr.SizeX;
            var fieldLength = ydimensionSize > xdimensionSize ? ydimensionSize : xdimensionSize;
            while (textLength > CalculateCharsInDots(
                       fieldLength,
                       fontIndex,
                       doubleWidth))
            {
                // remove excessive characters from the right.
                attributeDpr.DefaultText = attributeDpr.DefaultText.Substring(0, attributeDpr.DefaultText.Length - 1);
                textLength = attributeDpr.DefaultText.Length;
            }

            return attributeDpr.DefaultText;
        }
    }
}
