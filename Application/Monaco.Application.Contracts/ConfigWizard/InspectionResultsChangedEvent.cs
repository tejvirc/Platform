namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    using Kernel;

    /// <summary>
    ///     Inspection Results Changed event
    /// </summary>
    public class InspectionResultsChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor of event
        /// </summary>
        /// <param name="inspectionResult">Current data</param>
        public InspectionResultsChangedEvent(InspectionResultData inspectionResult)
        {
            InspectionResult = inspectionResult;
        }

        /// <summary>
        ///     Get the current data.
        /// </summary>
        public InspectionResultData InspectionResult { get; }
    }
}
