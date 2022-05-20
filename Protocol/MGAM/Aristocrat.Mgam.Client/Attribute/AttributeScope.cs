namespace Aristocrat.Mgam.Client.Attribute
{
    using System.ComponentModel;

    /// <summary>
    ///     
    /// </summary>
    public enum AttributeScope
    {
        /// <summary>Site.</summary>
        [Description("site")]
        Site,

        /// <summary>System.</summary>
        [Description("system")]
        System,

        /// <summary>Device.</summary>
        [Description("device")]
        Device,

        /// <summary>Application.</summary>
        [Description("application")]
        Application,

        /// <summary>Installation.</summary>
        [Description("installation")]
        Installation,

        /// <summary>Instance.</summary>
        [Description("instance")]
        Instance
    }
}
