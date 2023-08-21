namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    using System;

    /// <summary>Definition of the ButtonDownEvent class.</summary>
    [Serializable]
    public class DownEvent : ButtonBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DownEvent" /> class.
        /// </summary>
        public DownEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DownEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical button ID.</param>
        public DownEvent(int logicalId)
            : base(logicalId)
        {
        }
    }
}