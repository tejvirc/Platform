namespace Aristocrat.Mgam.Client.Notification
{
    using System;

    /// <summary>
    ///     Supported recipient codes.
    /// </summary>
    [Flags]
    public enum NotificationRecipientCode
    {
        /// <summary>Issue is logged to standard issue log.</summary>
        Log = 0x00010000,

        ///<summary>Race-track.</summary>
        Site = 0x00020000,

        ///<summary>Race-track security.</summary>
        Security = 0x00040000,

        /// <summary>Vendor or vendor representative.</summary>
        Vendor = 0x00080000,

        /// <summary>Central System.</summary>
        System = 0x00100000,

        /// <summary>New York Lottery.</summary>
        Lottery = 0x00200000,

        /// <summary>Multimedia Games.</summary>
        MultimediaGames = 0x00400000,

        /// <summary>Player tracking system.</summary>
        PlayerTracking = 0x00800000
    }
}
