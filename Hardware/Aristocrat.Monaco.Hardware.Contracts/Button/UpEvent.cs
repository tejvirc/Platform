namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    using System;

    /// <summary>Definition of the ButtonUpEvent class.</summary>
    [Serializable]
    public class UpEvent : ButtonBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UpEvent" /> class.
        /// </summary>
        public UpEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpEvent" /> class.
        /// </summary>
        /// <param name="logicalId">The logical button ID.</param>
        public UpEvent(int logicalId)
            : base(logicalId)
        {
        }
    }
}