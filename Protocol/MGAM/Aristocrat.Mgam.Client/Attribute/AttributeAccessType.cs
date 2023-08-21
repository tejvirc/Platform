namespace Aristocrat.Mgam.Client.Attribute
{
    using System;

    /// <summary>
    ///     Attribute access type for entities . All entities have read access.
    ///     Write Permission (0 = read only, 1 = read/write).
    /// </summary>
    [Flags]
    public enum AttributeAccessType
    {
        /// <summary>
        ///     No access.
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Vlt read/write access.
        /// </summary>
        Device = 0x01,

        /// <summary>
        ///     Site-controller read/write access.
        /// </summary>
        SiteController = 0x02,

        /// <summary>
        ///     Management read/write access.
        /// </summary>
        Management = 0x04
    }
}
