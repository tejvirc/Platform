namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;

    /// <summary>
    ///     Definition of the AlteredMediaLogInfo class.
    /// </summary>
    public class AlteredMediaLogInfo
    {
        /// <summary>
        ///     Gets or sets the date and time the Media is altered.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets the Media type
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        ///     Gets or sets the reason for media change
        /// </summary>
        public string ReasonForChange { get; set; }

        /// <summary>
        ///     Gets or sets the authentication information for the media
        /// </summary>
        public string Authentication { get; set; }
    }
}