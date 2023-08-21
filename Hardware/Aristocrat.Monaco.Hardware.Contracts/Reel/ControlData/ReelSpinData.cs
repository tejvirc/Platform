namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This incorporates the data required for spinning a reel
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Data will be used once wired up to the game")]
    public class ReelSpinData : ISpinData
    {
        /// <summary>
        ///     Creates the reel spin data
        /// </summary>
        /// <param name="reelId">The reel Id for this spin details</param>
        /// <param name="direction">The direction for the reel to spin</param>
        /// <param name="rpm">The rpm to spin the reels</param>
        /// <param name="step">The step for the reel if using the controller timing or -1 if you are spinning until told to stop</param>
        public ReelSpinData(int reelId, SpinDirection direction, int rpm = -1, int step = -1)
        {
            ReelId = reelId;
            Direction = direction;
            Rpm = rpm; // negative RPM will indicate use the default speed for the controller
            Step = step; // negative step will indicate spinning freely.  Stop timing is game controlled and firmware controlled
        }

        ///  <inheritdoc />
        public int ReelId { get; set; }

        ///  <inheritdoc />
        public SpinDirection Direction { get; set; }

        ///  <inheritdoc />
        public int Rpm { get; set;  }

        ///  <inheritdoc />
        public int Step { get; set;  }
    }
}