namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;

    /// <summary>
    ///     The PrepareStepperCurves class.
    /// </summary>
    public class PrepareStepperCurves
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrepareStepperCurves" /> class.
        /// </summary>
        /// <param name="reelCurveData">The reel curve data</param>
        public PrepareStepperCurves(params ReelCurveData[] reelCurveData)
        {
            ReelCurveData = reelCurveData;
        }

        /// <summary>
        ///     Gets the reel curve data
        /// </summary>
        public ReelCurveData[] ReelCurveData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the stepper curves were prepared
        /// </summary>
        public bool Success { get; set; }
    }
}
