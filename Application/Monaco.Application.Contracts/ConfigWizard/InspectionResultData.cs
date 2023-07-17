namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    using System.Linq;
    using System.Collections.Generic;
    using HardwareDiagnostics;

    /// <summary>
    ///     Inspection result data
    /// </summary>
    public class InspectionResultData
    {
        /// <summary>
        ///     Device category
        /// </summary>
        public HardwareDiagnosticDeviceCategory Category { get; set; }

        /// <summary>
        ///     List of firmware versions
        /// </summary>
        public IList<string> FirmwareVersions { get; set; }

        /// <summary>
        ///     Page test status
        /// </summary>
        public InspectionPageStatus Status { get; set; }

        /// <summary>
        ///     List of failure messages
        /// </summary>
        public IList<string> FailureMessages { get; set; }

        /// <summary>
        ///     Get combined firmware versions string.
        /// </summary>
        public string CombinedFirmwareVersions(string delimiter) => string.Join(delimiter, FirmwareVersions.Distinct());

        /// <summary>
        ///     Get combined test failures string.
        /// </summary>
        public string CombinedTestFailures(string delimiter) => string.Join(delimiter, FailureMessages.Distinct());
    }
}
