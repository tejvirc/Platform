////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="PrintableRegion.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2010 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using System;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     <para>
    ///         PrintableRegion is an area (content block) on the printed output that is self described by
    ///         the syntax of the PDL (Page Description Language) and takes on the printed form of text, barcodes or graphics.
    ///     </para>
    ///     <para>
    ///         Printer output is defined in terms of printable regions, or content blocks.
    ///         Each printable region has certain required properties that describe its uniqueness.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         PDL Command Descriptions used for the PrintableRegion is mentioned below.
    ///         The following is the structure of the DPR command. ("value" would be replaced with the appropriate property
    ///         value).
    ///     </para>
    ///     <see>.\Hardware\Aristocrat.Monaco.Hardware\printable_regions.xml</see>
    ///     <code><DPR id="value" x="value" y="value" dx="value" dy="value" rot="value" jst="value" type="value" m1="value"
    ///               m2="value" attr="value">
    ///         <D>value</D>
    ///     </DPR></code>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>id</term>
    ///                 <description>
    ///                     <para>Three digit printable region identifier.</para>
    ///                     <para>Value ranges from 000 to 999.</para>
    ///                     <para>000 – 099 are reserved for predefined regions.</para>
    ///                     <para>100 – 999 reserved for downloadable use.</para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>x</term>
    ///                 <description>
    ///                     <para>Five digit X (dot) axis start position in dots.</para>
    ///                     <para>Value ranges from 00000 to 65536.</para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>y</term>
    ///                 <description>
    ///                     <para>Five digit Y (paper) axis start position in dots.</para>
    ///                     <para>Value ranges from 00000 to 65536.</para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>dx</term>
    ///                 <description>
    ///                     <para>Five digit X (dot) axis length of the printable region in dots.</para>
    ///                     <para>Value ranges from 00000 to 65536.</para>
    ///                     <para>
    ///                         Note that x + dx must not overflow the width of the print head in dots; else,
    ///                         the content block will be rejected and a printable region truncation error will result.
    ///                     </para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>dy</term>
    ///                 <description>
    ///                     <para>Five digit Y (paper) axis length of printable region in dots.</para>
    ///                     <para>Value ranges from 00000 to 65536.</para>
    ///                     <para>
    ///                         Note that the y + dy may not exceed printable area in dots. If this is the case,
    ///                         the printable region will be rejected and a printable region truncation error will result.
    ///                     </para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>rot</term>
    ///                 <description>
    ///                     <para>One digit rotation of strings or data within printable region.</para>
    ///                     <para>Value ranges from 1 to 4. (1 = 0 degree, 2 = 90 degree, 3 = 180 degree and 4 = 270 degree).</para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>jst</term>
    ///                 <description>
    ///                     <para>
    ///                         One digit justification identifier. Defines justification of data within
    ///                         printable region with respect to top of printable region.
    ///                     </para>
    ///                     <para>
    ///                         Value ranges from 1 to 3. (1 = left justification, 2 = center justification, 3 = right
    ///                         justification).
    ///                     </para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>type</term>
    ///                 <description>
    ///                     <para>
    ///                         Three part printable region identifier, comprised of a letter, an equal sign, and three-digit
    ///                         value
    ///                         that points to an element in the data structure of Printer Metrics.
    ///                     </para>
    ///                     <see>The GDS Printer Communication Protocol.</see>
    ///                     <para>
    ///                         This is the print object (barcode, font or graphic) used to format the data from the command to
    ///                         define
    ///                         the template.
    ///                     </para>
    ///                     <para>
    ///                         1st part: F, G, or B. (F, font: Used when the printable region holds text.
    ///                         G, graphics: Used when printable region holds graphics.
    ///                         B, barcode: Used when printable region holds barcode.).
    ///                     </para>
    ///                     <para>2nd part: = (equal sign).</para>
    ///                     <para>3rd part: 000 to 999.</para>
    ///                     <para>
    ///                         F=005 : Points to Font [5] in data structure. G=105 : Points to Graphic [105] in data
    ///                         structure. B=015
    ///                         : Points to Barcode Symbology [015] in data structure.
    ///                     </para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>m1</term>
    ///                 <description>
    ///                     <para>One digit print object multiplier 1.</para>
    ///                     <para>Value ranges from 1 to 6 for text or barcode and 0 for graphics.</para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>m2</term>
    ///                 <description>
    ///                     <para>Print object multiplier 2.</para>
    ///                     <para>Value ranges from 1 to 6 for text, 0 or 2 to 24 for barcode and 0 for graphics.</para>
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <term>attr</term>
    ///                 <description>
    ///                     <para>
    ///                         Three digit object printing attribute. This contains special instructions on
    ///                         how to treat the print objects within the printable region.
    ///                     </para>
    ///                     <para>Value ranges from 001 or 002 for text, 048 or 406 for barcode and 000 for graphics.</para>
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public class PrintableRegion : IComparable
    {
        /// <summary>Initializes a new instance of the <see cref="PrintableRegion" /> class.</summary>
        /// <param name="name">The property name.</param>
        /// <param name="property">The property.</param>
        /// <param name="id">The printable region identifier value.</param>
        /// <param name="originX">The X (dot) axis start position in dots.</param>
        /// <param name="pdlOriginX">The PDL X (dot) axis start position in dots.</param>
        /// <param name="originY">The Y (paper) axis start position in dots.</param>
        /// <param name="pdlOriginY">The PDL Y (paper) axis start position in dots.</param>
        /// <param name="sizeX">The X (dot) axis length of the printable region in dots.</param>
        /// <param name="pdlSizeX">The PDL X (dot) axis length of the printable region in dots.</param>
        /// <param name="sizeY">The Y (paper) axis length of printable region in dots.</param>
        /// <param name="pdlSizeY">The PDL Y (paper) axis length of printable region in dots.</param>
        /// <param name="rotation">The rotation of strings or data within printable region.</param>
        /// <param name="justification">
        ///     The justification identifier. Defines justification of data
        ///     within printable region with respect to top of printable region.
        /// </param>
        /// <param name="format">
        ///     The printable region identifier, comprised of
        ///     a letter, an equal sign, and three-digit value that points to an element in the data structure of Printer Metrics.
        /// </param>
        /// <param name="modifier1">The print object multiplier 1.</param>
        /// <param name="modifier2">The print object multiplier 2.</param>
        /// <param name="attribute">The object printing attribute.</param>
        /// <param name="defaultValue">The default Value.</param>
        public PrintableRegion(
            string name,
            string property,
            int id,
            int originX,
            string pdlOriginX,
            int originY,
            string pdlOriginY,
            int sizeX,
            string pdlSizeX,
            int sizeY,
            string pdlSizeY,
            int rotation,
            int justification,
            string format,
            int modifier1,
            int modifier2,
            string attribute,
            string defaultValue)
        {
            Name = name;
            Property = property;
            Id = id;
            OriginX = originX;
            PdlOriginX = pdlOriginX;
            OriginY = originY;
            PdlOriginY = pdlOriginY;
            SizeX = sizeX;
            PdlSizeX = pdlSizeX;
            SizeY = sizeY;
            PdlSizeY = pdlSizeY;
            Rotation = rotation;
            Justification = justification;
            Format = format;
            Modifier1 = modifier1;
            Modifier2 = modifier2;
            Attribute = attribute;
            DefaultText = defaultValue;
        }

        /// <summary>Gets or sets the property Name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the Property.</summary>
        public string Property { get; set; }

        /// <summary>Gets or sets the printable region identifier value.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the X (dot) axis start position in dots.</summary>
        public int OriginX { get; set; }

        /// <summary>Gets or sets the PDL X (dot) axis start position in dots.</summary>
        public string PdlOriginX { get; set; }

        /// <summary>Gets or sets the Y (paper) axis start position in dots.</summary>
        public int OriginY { get; set; }

        /// <summary>Gets or sets the PDL Y (paper) axis start position in dots.</summary>
        public string PdlOriginY { get; set; }

        /// <summary>Gets or sets the X (dot) axis length of the printable region in dots.</summary>
        public int SizeX { get; set; }

        /// <summary>Gets or sets the PDL X (dot) axis length of the printable region in dots.</summary>
        public string PdlSizeX { get; set; }

        /// <summary>Gets or sets the Y (paper) axis length of printable region in dots.</summary>
        public int SizeY { get; set; }

        /// <summary>Gets or sets the PDL Y (paper) axis length of printable region in dots.</summary>
        public string PdlSizeY { get; set; }

        /// <summary>Gets or sets the rotation of strings or data within printable region.</summary>
        public int Rotation { get; set; }

        /// <summary>Gets or sets the justification identifier.</summary>
        public int Justification { get; set; }

        /// <summary>Gets or sets Format.</summary>
        public string Format { get; set; }

        /// <summary>Gets or sets the print object multiplier 1.</summary>
        public int Modifier1 { get; set; }

        /// <summary>Gets or sets the print object multiplier 2.</summary>
        public int Modifier2 { get; set; }

        /// <summary>Gets or sets the object printing attribute.</summary>
        public string Attribute { get; set; }

        /// <summary>Gets or sets DefaultText.</summary>
        public string DefaultText { get; set; }

        /// <summary>Gets the value for next page from region.</summary>
        public int NextPage
        {
            get
            {
                if (!GetContextValue('1', 5, out var page))
                {
                    GetContextValue('2', 5, out page);
                }

                return page;
            }
        }

        /// <summary>Compares this instance to a <see cref="PrintableRegion" /> and returns an indication of their relative value.</summary>
        /// <param name="obj">The object compare against.</param>
        /// <returns>Indication of relative values.</returns>
        /// <exception cref="ArgumentException">Throw an ArgumentException</exception>
        public int CompareTo(object obj)
        {
            var compareRegion = obj as PrintableRegion;
            if (compareRegion != null)
            {
                return OriginX.CompareTo(compareRegion.OriginX);
            }

            throw new ArgumentException("Object is not a PrintableRegion.");
        }

        /// <summary>Override of equality test for <see cref="PrintableRegion" /></summary>
        /// <param name="left">The left object.</param>
        /// <param name="right">The right object.</param>
        /// <returns>True if both objects are equal.</returns>
        public static bool operator ==(PrintableRegion left, PrintableRegion right)
        {
            return Equals(left, right);
        }

        /// <summary>Override of inequality test for <see cref="PrintableRegion" />.</summary>
        /// <param name="left">The left object.</param>
        /// <param name="right">The right object.</param>
        /// <returns>True if both objects are not equal.</returns>
        public static bool operator !=(PrintableRegion left, PrintableRegion right)
        {
            return !Equals(left, right);
        }

        /// <summary>Greater than test for <see cref="PrintableRegion" />.</summary>
        /// <param name="left">The left object.</param>
        /// <param name="right">The right object.</param>
        /// <returns>True if the left region Id is greater than the right region Id.</returns>
        public static bool operator >(PrintableRegion left, PrintableRegion right)
        {
            return left.Id > right.Id;
        }

        /// <summary>Less than test for <see cref="PrintableRegion" />.</summary>
        /// <param name="left">The left object.</param>
        /// <param name="right">The right object.</param>
        /// <returns>True if the left region Id is less than the right region Id.</returns>
        public static bool operator <(PrintableRegion left, PrintableRegion right)
        {
            return left.Id < right.Id;
        }

        /// <summary>Checks whether region is flagged as optional.</summary>
        /// <returns>True if region is optional.</returns>
        public bool IsFlagged()
        {
            return FindAttribute("903");
        }

        /// <summary>Checks whether next page is optional.</summary>
        /// <returns>True if next page is optional.</returns>
        public bool IsOptionalNextPage()
        {
            var page = -1;

            return GetContextValue('2', 5, out page);
        }

        /// <summary>Checks if this region is optional</summary>
        /// <returns>True if the region is optional.</returns>
        public bool IsOptionalRegion()
        {
            return FindAttribute("901") || FindAttribute("902");
        }

        /// <summary>Checks if this region is optional and static.</summary>
        /// <returns>True if the region is option and static.</returns>
        public bool IsOptionalStaticRegion()
        {
            return FindAttribute("901");
        }

        /// <summary>Searches string delimited by '+' for substring match.</summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>True if attribute is found.</returns>
        public bool FindAttribute(string attribute)
        {
            return Attribute != null && Attribute.Split('+').Any(s => s == attribute);
        }

        /// <summary>Checks to see if type of specified length is in region and sets as value.</summary>
        /// <param name="type">The first character of value.</param>
        /// <param name="length">The length of value.</param>
        /// <param name="value">The reference to value.</param>
        /// <returns>
        ///     Boolean indicating if source contains substring of type and
        ///     length passed in.
        /// </returns>
        public bool GetContextValue(char type, int length, out int value)
        {
            var found = false;
            value = -1;
            if (!string.IsNullOrEmpty(Attribute))
            {
                foreach (var s in Attribute.Split('+').Where(s => s.Length == length && s[0] == type))
                {
                    found = true;
                    value = int.Parse(s.Substring(1), CultureInfo.InvariantCulture);
                    break;
                }
            }

            return found;
        }

        /// <summary>Compares this instance to a <see cref="PrintableRegion" /> and checks for equality.</summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(PrintableRegion))
            {
                return false;
            }

            return Equals((PrintableRegion)obj);
        }

        /// <summary>Compares this instance to a <see cref="PrintableRegion" /> and returns an indication of their relative value.</summary>
        /// <param name="rightRegion">The right region.</param>
        /// <param name="comparisonMethod">The comparison method.</param>
        /// <returns>Indication of relative values.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Throws an ArgumentOutOfRangeException:
        ///     <paramref name="comparisonMethod" />
        /// </exception>
        public int CompareTo(PrintableRegion rightRegion, ComparisonType comparisonMethod)
        {
            switch (comparisonMethod)
            {
                case ComparisonType.PositionComparisonPortrait:
                case ComparisonType.LineComparisonLandscape:
                    return OriginX.CompareTo(rightRegion.OriginX);

                case ComparisonType.PositionComparisonLandscape:
                case ComparisonType.LineComparisonPortrait:
                    return OriginY.CompareTo(rightRegion.OriginY);

                default:
                    throw new ArgumentOutOfRangeException(nameof(comparisonMethod));
            }
        }

        /// <summary>
        ///     Returns the string representation of the PDL DPR command.
        /// </summary>
        /// <param name="increaseFont">Use larger font size.</param>
        /// <returns>The string representation of the PDL DPR command.</returns>
        public string ToPDL(bool increaseFont)
        {
            var part1 = string.Format(
                CultureInfo.InvariantCulture,
                "<DPR id=\"{0}\" x=\"{1}\" y=\"{2}\" dx=\"{3}\" dy=\"{4}\" rot=\"{5}\" ",
                Id,
                PdlOriginX,
                PdlOriginY,
                PdlSizeX,
                PdlSizeY,
                Rotation);
            var part2 = string.Format(
                CultureInfo.InvariantCulture,
                "jst=\"{0}\" type=\"{1}\" m1=\"{2}\" m2=\"{3}\" attr=\"{4}\"><D>{5}</D></DPR>",
                Justification,
                PdlAdjustType(Format, increaseFont),
                Modifier1,
                Modifier2,
                Attribute,
                DefaultText);
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", part1, part2);
        }

        /// <summary>Checks for equality between two <see cref="PrintableRegion" />.</summary>
        /// <param name="other">The other region.</param>
        /// <returns>True if both objects are equivalent.</returns>
        public bool Equals(PrintableRegion other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.Name, Name) &&
                   Equals(other.Property, Property) &&
                   other.Id == Id &&
                   other.OriginX == OriginX &&
                   other.OriginY == OriginY &&
                   other.SizeX == SizeX &&
                   other.SizeY == SizeY &&
                   other.Rotation == Rotation &&
                   other.Justification == Justification &&
                   Equals(other.Format, Format) &&
                   other.Modifier1 == Modifier1 &&
                   other.Modifier2 == Modifier2 &&
                   Equals(other.Attribute, Attribute) &&
                   Equals(other.DefaultText, DefaultText);
        }

        /// <summary>Serves as a hash function for a particular type.</summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = Name != null ? Name.GetHashCode() : 0;
                result = (result * 397) ^ (Property != null ? Property.GetHashCode() : 0);
                result = (result * 397) ^ Id;
                result = (result * 397) ^ OriginX;
                result = (result * 397) ^ OriginY;
                result = (result * 397) ^ SizeX;
                result = (result * 397) ^ SizeY;
                result = (result * 397) ^ Rotation;
                result = (result * 397) ^ Justification;
                result = (result * 397) ^ (Format != null ? Format.GetHashCode() : 0);
                result = (result * 397) ^ Modifier1;
                result = (result * 397) ^ Modifier2;
                result = (result * 397) ^ (Attribute != null ? Attribute.GetHashCode() : 0);
                result = (result * 397) ^ (DefaultText != null ? DefaultText.GetHashCode() : 0);
                return result;
            }
        }

        /// <summary>
        ///     Returns the string representation of the PrintableRegion
        /// </summary>
        /// <returns>The string representation of the PrintableRegion</returns>
        public override string ToString()
        {
            var part1 = string.Format(
                CultureInfo.InvariantCulture,
                "PrintableRegion [Name: {0}, Property: {1}, Id: {2}, OriginX: {3}, ",
                Name,
                Property,
                Id,
                OriginX);
            var part2 = string.Format(
                CultureInfo.InvariantCulture,
                "OriginY: {0}, SizeX: {1}, SizeY: {2}, Rotation: {3}, Justification: {4}, ",
                OriginY,
                SizeX,
                SizeY,
                Rotation,
                Justification);
            var part3 = string.Format(
                CultureInfo.InvariantCulture,
                "Format: {0}, Modifier1: {1}, Modifier2: {2}, Attribute: {3}, DefaultText: {4}",
                Format,
                Modifier1,
                Modifier2,
                Attribute,
                DefaultText);
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", part1, part2, part3);
        }

        private string PdlAdjustType(string type, bool increaseFontSize)
        {
            var pdlType = type;

            var delimiter = '=';
            var segments = type.Split(delimiter);
            if (segments.Length == 2)
            {
                switch (segments[0])
                {
                    case "F": // Fonts
                        pdlType = "F=";
                        if (segments[1].Equals("009"))
                        {
                            pdlType += "008"; // Font 009 is changed to 008 for PDL
                        }
                        else if (segments[1].Equals("008"))
                        {
                            pdlType += !increaseFontSize ? "007" : "005"; // Font 008 is changed to 007 or 005 for PDL
                        }
                        else
                        {
                            pdlType += segments[1];
                        }

                        break;
                    default:
                        break;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(type, @"Invalid DPR type " + type);
            }

            return pdlType;
        }
    }
}