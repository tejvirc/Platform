using static System.FormattableString;

namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Hopper;
    using Kernel;

    /// <summary>
    ///     Used for Hopper emulation
    /// </summary>
    public class FakeHopperFaultEvent : BaseEvent
    {
        /// <summary>Gets or sets the identifier of the Hopper Fault.</summary>
        /// <value>Hopper fault.</value>
        public HopperFaultTypes FaultType { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [HopperFaultTypes={FaultType}]");
        }
    }
}