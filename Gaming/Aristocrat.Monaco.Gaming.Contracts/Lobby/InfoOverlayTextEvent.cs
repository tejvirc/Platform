namespace Aristocrat.Monaco.Gaming.Contracts.Lobby
{
    using System;
    using Kernel;
    using Models;

    /// <summary>
    ///     Published when an informational overlay message should be displayed (like Demo or Developer)
    /// </summary>
    public class InfoOverlayTextEvent : BaseEvent
    {
        /// <summary>
        ///     Gets or sets text to set
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Gets or sets text guid to set
        /// </summary>
        public Guid TextGuid { get; set; }

        /// <summary>
        ///     Gets or sets the location to send the text
        /// </summary>
        public InfoLocation Location { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to remove the text indicated by the GUID
        /// </summary>
        public bool Clear { get; set; }
    }
}
