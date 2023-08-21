namespace Aristocrat.Monaco.Accounting.UI.Models
{
    using System;

    /// <summary>
    ///     Definition of the AlteredMediaLogData class.
    /// </summary>
    public class AlteredMediaLogData
    {
        /// <summary>
        ///     Gets or sets the date and time the Media is altered.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the Media type
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the reason for media change
        /// </summary>
        public string ReasonForChange { get; set; }

        /// <summary>
        /// Gets or sets the authentication information for media change
        /// </summary>
        public string AuthenticationInfo { get; set; }
    }
}
