namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     The reel status class.
    /// </summary>
    public class ReelStatus
    {
        /// <summary>
        ///     Gets or sets the reel id for the status
        /// </summary>
        public int ReelId { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the reel has stalled
        /// </summary>
        public bool ReelStall { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the reel has been tampered with
        /// </summary>
        public bool ReelTampered { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the reel is connected
        /// </summary>
        public bool Connected { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a reel requested to spin/nudge to goal resulted in low voltage.
        /// </summary>
        public bool LowVoltage { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a reel requested to home resulted in an error.
        /// </summary>
        public bool FailedHome { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a reel optic sequence error has occurred.
        /// </summary>
        public bool OpticSequenceError { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating when a reel stops at an unknown stop
        /// </summary>
        public bool IdleUnknown { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating when it is required to stop at an unknown stop.
        /// </summary>
        public bool UnknownStop { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder($"{nameof(ReelStatus)} {{ {nameof(ReelId)} = {ReelId}");
            sb.Append($", {nameof(Connected)} = {Connected}");

            if (ReelStall)
            {
                sb.Append(", Stalled");
            }
            if (ReelTampered)
            {
                sb.Append(", Tampered");
            }
            if (LowVoltage)
            {
                sb.Append(", LowVoltage");
            }
            if (FailedHome)
            {
                sb.Append(", FailedHome");
            }
            if (OpticSequenceError)
            {
                sb.Append(", OpticSequenceError");
            }
            if (IdleUnknown)
            {
                sb.Append(", IdleUnknown");
            }
            if (UnknownStop)
            {
                sb.Append(", UnknownStop");
            }

            sb.Append(" }");
            return sb.ToString();
        }

        /// <summary>
        ///     Gets the faults for this reel status.
        /// </summary>
        /// <returns>Reel faults</returns>
        public ReelFaults GetFaults()
        {
            return new[]
                {
                    (LowVoltage, ReelFaults.LowVoltage),
                    (ReelTampered, ReelFaults.ReelTamper),
                    (ReelStall, ReelFaults.ReelStall),
                    (OpticSequenceError, ReelFaults.ReelOpticSequenceError),
                    (IdleUnknown, ReelFaults.IdleUnknown),
                    (UnknownStop, ReelFaults.UnknownStop)
                }
                .Where(x => x.Item1)
                .Aggregate(ReelFaults.None, (sum, current) => sum | current.Item2);
        }
    }
}
