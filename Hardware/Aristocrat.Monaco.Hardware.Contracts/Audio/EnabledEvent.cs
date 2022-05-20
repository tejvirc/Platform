namespace Aristocrat.Monaco.Hardware.Contracts.Audio
{
    using System;
    using Kernel;
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the Audio EnabledEvent class.</summary>
    /// <remarks>This event is posted when Audio device becomes Enabled.</remarks>
    [Serializable]
    public class EnabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(EnabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the enabled event.</summary>
        public EnabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"Audio {base.ToString()} {Reasons}");
        }
    }
}