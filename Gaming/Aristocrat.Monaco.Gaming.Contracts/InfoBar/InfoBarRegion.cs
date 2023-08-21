namespace Aristocrat.Monaco.Gaming.Contracts.InfoBar
{
    /// <summary>
    ///     The InfoBar is divided into three equal regions, each able to independently display messages.
    /// </summary>
    /// <remarks>
    ///     See G2S Message Protocol v3.0, Appendix E, Section 4.2
    /// </remarks>
    public enum InfoBarRegion
    {
        /// <summary>
        ///     The left region of the InfoBar
        /// </summary>
        Left,

        /// <summary>
        ///     The left region of the InfoBar
        /// </summary>
        Center,

        /// <summary>
        ///     The left region of the InfoBar
        /// </summary>
        Right
    }
}