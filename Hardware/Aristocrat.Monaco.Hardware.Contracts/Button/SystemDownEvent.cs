namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    using System;

    /// <summary>Definition of the SystemDownEvent class. The Event is ignoring button states</summary>
    [Serializable]
    public class SystemDownEvent : DownEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDownEvent" /> class.
        /// </summary>
        public SystemDownEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDownEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical button ID.</param>
        /// <param name="isEnabled">The system button state.</param>
        public SystemDownEvent(int logicalId, bool isEnabled = false)
            : base(logicalId)
        {
            Enabled = isEnabled;
        }

        /// <summary>
        ///     State of the system button.
        /// </summary>
        public bool Enabled { get; set; }
    }
}