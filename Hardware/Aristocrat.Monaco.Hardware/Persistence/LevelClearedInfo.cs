namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;

    /// <summary> A class for level clear information. </summary>
    public class LevelClearedInfo
    {
        /// <summary> Gets or sets the last clear time. </summary>
        /// <value> The last clear time. </value>
        public DateTime LastClearTime { get; set; }

        /// <summary> Gets or sets a value indicating whether the just executed. </summary>
        /// <value> True if just executed, false if not. </value>
        public bool JustExecuted { get; set; }
    }
}