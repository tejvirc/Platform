namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;
    using System.Collections.Generic;

    /// <summary>
    ///     Prepares StepperCurves
    /// </summary>
    public class PrepareStepperCurves
    {
        public IReadOnlyCollection<ReelCurveData> StepperCurvesData;

        /// <summary>
        ///     Gets or sets a value indicating whether or not the stepper curves were prepared
        /// </summary>
        public bool Success { get; set; }
    }
}
