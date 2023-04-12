namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This incorporates the data required for nudging a reel
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Data will be used once wired up to the game")]
    public class NudgeReelData : ISpinData
    {
        /// <summary>
        ///     Creates the nudge reel data
        /// </summary>
        /// <param name="reelId">The reel Id for this spin details</param>
        /// <param name="direction">The direction for the reel to spin</param>
        /// <param name="step">The step for the reel if using the controller timing</param>
        /// <param name="rpm">The rpm to spin the reels</param>
        /// <param name="delay">The delay before spinning the reels</param>
        public NudgeReelData(int reelId, SpinDirection direction, int step, int rpm = -1, int delay = -1)
        {
            ReelId = reelId;
            Direction = direction;
            Rpm = rpm; // negative RPM will indicate use the default speed for the controller
            Delay = delay; // negative delay will indicate use the default delay for the controller
            Step = step;
        }

        ///  <inheritdoc />
        public int ReelId { get; set; }

        ///  <inheritdoc />
        public SpinDirection Direction { get; set; }

        ///  <inheritdoc />
        public int Rpm { get; set; }

        ///  <inheritdoc />
        public int Step { get; set; }

        /// <summary>
        ///     The delay before spinning the reels or -1 if using the default on the controller
        /// </summary>
        public int Delay { get; set; }
    }
}