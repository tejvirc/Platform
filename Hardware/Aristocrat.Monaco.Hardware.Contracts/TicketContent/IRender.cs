namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using System;
    using System.Collections.Generic;

    #region Enumerations

    /// <summary>
    ///     Printer types that may be rendered to.
    /// </summary>
    public enum RenderTarget
    {
        /// <summary>No render target.</summary>
        None = 0,

        /// <summary>Render ticket for TransAct Epic 950 printer.</summary>
        Epic950,

        /// <summary>Render ticket for Ithaca 850 printer.</summary>
        Ithaca850,

        /// <summary>Render ticket for Ithaca printer.</summary>
        Ithaca,

        /// <summary>Render ticket for Paycheck 3 printer.</summary>
        Paycheck3,

        /// <summary>Render ticket for Paycheck FX printer.</summary>
        PaycheckFX
    }

    /// <summary>
    ///     Special enumerations for rendering.
    /// </summary>
    public enum RenderBaseEnumeration
    {
        /// <summary>
        ///     Default value of the RenderBaseEnumeration indicating no Value is set
        /// </summary>
        None = 0,

        /// <summary>Special print region id for blank line.</summary>
        BlankLineDprid = 99999,

        /// <summary>The page width used for landscape mode.</summary>
        LandscapePageWidth = 1268
    }

    /// <summary>Region print type.</summary>
    public enum PrintType
    {
        /// <summary>The region holds text.</summary>
        Font = 0,

        /// <summary>The region holds graphics.</summary>
        Graphics,

        /// <summary>The region holds a barcode.</summary>
        Barcode,

        /// <summary>The type is unknown.</summary>
        Unknown
    }

    /// <summary>Print region rotation.</summary>
    public enum RegionRotation
    {
        /// <summary>Unknown rotation.</summary>
        Unknown = 0,

        /// <summary>0 degrees rotation.</summary>
        Rotation0,

        /// <summary>90 degrees rotation.</summary>
        Rotation90,

        /// <summary>180 degrees rotation.</summary>
        Rotation180,

        /// <summary>270 degrees rotation.</summary>
        Rotation270
    }

    /// <summary>Print alignment.</summary>
    public enum PrintAlignment
    {
        /// <summary>No alignment.</summary>
        None = 0,

        /// <summary>Left aligned.</summary>
        Left,

        /// <summary>Center aligned.</summary>
        Center,

        /// <summary>Right aligned.</summary>
        Right
    }

    /// <summary>Character size mode.</summary>
    public enum CharSizeMode
    {
        /// <summary>Normal character size.</summary>
        Normal = 0,

        /// <summary>Doubled character size.</summary>
        Double
    }

    /// <summary>Print orientation for ticket rendering.</summary>
    public enum PrintOrientation
    {
        /// <summary>Portrait orientation.</summary>
        Portrait = 0,

        /// <summary>Landscape orientation.</summary>
        Landscape
    }

    /// <summary>Indicates the font size in Characters per inch.</summary>
    public enum RawFontType
    {
        /// <summary>Unknown size.</summary>
        Unknown = 0,

        /// <summary>7 CPI font.</summary>
        Cpi7,

        /// <summary>10 CPI font.</summary>
        Cpi10,

        /// <summary>12 CPI font.</summary>
        Cpi12,

        /// <summary>16 CPI font.</summary>
        Cpi16,

        /// <summary>20 CPI font.</summary>
        Cpi20
    }

    #endregion

    /// <summary>
    ///     Definition of the IRender interface.
    /// </summary>
    [CLSCompliant(false)]
    [Obsolete]
    public interface IRender
    {
        /// <summary>Gets raw print data as a result of the rendering of a ticket.</summary>
        /// <returns>String of raw print data for printer.</returns>
        string RawData { get; }

        /// <summary>Render multiple pages.</summary>
        /// <param name="pages">A list of pages.</param>
        /// <returns>True if successful.</returns>
        bool RenderPages(IList<IList<PrintableRegion>> pages);

        /// <summary>Render one page.</summary>
        /// <param name="page">The page to render.</param>
        /// <returns>True if successful.</returns>
        bool Render(IList<PrintableRegion> page);

        /// <summary>Get the region's type and value.</summary>
        /// <param name="type">The source string containing type and value.</param>
        /// <returns> Regiontype Information</returns>
        RegionType TranslateType(string type);

        /// <summary>Assign the printing property for the current rendering session.</summary>
        /// <param name="properties">The properties.</param>
        void SetTicketProperties(Dictionary<string, string> properties);
    }
}