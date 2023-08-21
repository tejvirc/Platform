namespace Aristocrat.Monaco.Hardware.Tickets
{
    using System;
    using System.Globalization;
    using Contracts.TicketContent;

    /// <summary>Definition of the PrintableRegionExtensionMethods class.</summary>
    [Obsolete]
    public static class PrintableRegionExtensionMethods
    {
        /// <summary>Finds attribute as named in render base enumeration.</summary>
        /// <param name="region">The region.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>Result of search for attribute in region.</returns>
        public static bool FindAttribute(this PrintableRegion region, RenderBase.AttributeIdentifier attribute)
        {
            return region.FindAttribute(((int)attribute).ToString(CultureInfo.InvariantCulture));
        }
    }
}
