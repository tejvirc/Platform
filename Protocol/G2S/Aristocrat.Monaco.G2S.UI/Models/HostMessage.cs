namespace Aristocrat.Monaco.G2S.UI.Models
{
    using System;

    /// <summary>
    ///     Defines a G2S Host message
    /// </summary>
    public class HostMessage
    {
        /// <summary>
        ///     Gets or sets date received
        /// </summary>
        public DateTime DateReceived { get; set; }

        /// <summary>
        ///     Gets or sets  to location
        /// </summary>
        public string ToLocation { get; set; }

        /// <summary>
        ///     Gets or sets session type
        /// </summary>
        public string SessionType { get; set; }

        /// <summary>
        ///     Gets or sets session id
        /// </summary>
        public long SessionId { get; set; }

        /// <summary>
        ///     Gets or sets command id
        /// </summary>
        public long CommandId { get; set; }

        /// <summary>
        ///     Gets or sets device
        /// </summary>
        public string Device { get; set; }

        /// <summary>
        ///     Gets or sets summary
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        ///     Gets the associated error code
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        ///     Gets the associated raw data
        /// </summary>
        public string RawData { get; set; }
    }
}