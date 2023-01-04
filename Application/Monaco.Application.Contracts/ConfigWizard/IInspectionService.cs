namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    using System.Linq;
    using System.Collections.Generic;
    using HardwareDiagnostics;
    using Kernel;

    /// <summary>
    ///     Defines interface for inspection details
    /// </summary>
    public interface IInspectionService : IService
    {
        /// <summary>
        ///     Get a collection of inspection results
        /// </summary>
        ICollection<InspectionResultData> Results { get; }

        /// <summary>
        ///     Set up the device category for following tests.
        /// </summary>
        /// <param name="category">Device category</param>
        void SetDeviceCategory(HardwareDiagnosticDeviceCategory category);

        /// <summary>
        ///     Set the firmware version of current category.
        /// </summary>
        /// <param name="firmwareVersion">Firmware version</param>
        void SetFirmwareVersion(string firmwareVersion);

        /// <summary>
        ///     Set up the following test's name.
        /// </summary>
        /// <param name="testName">Test name</param>
        void SetTestName(string testName);

        /// <summary>
        ///     Report a test failure.
        /// </summary>
        void ReportTestFailure();
    }

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
        ///     Firmware version
        /// </summary>
        public string FirmwareVersion { get; set; }

        /// <summary>
        ///     Page test status
        /// </summary>
        public InspectionPageStatus Status { get; set; }

        /// <summary>
        ///     List of failure messages
        /// </summary>
        public IList<string> FailureMessages { get; set; }

        /// <summary>
        ///     Get combined test failures string.
        /// </summary>
        public string CombinedTestFailures => string.Join("; ", FailureMessages.Distinct());
    }

    /// <summary>
    ///     Enumeration of status of page testign
    /// </summary>
    public enum InspectionPageStatus
    {
        /// <summary>
        ///     Untested
        /// </summary>
        Untested,

        /// <summary>
        ///     Good
        /// </summary>
        Good,

        /// <summary>
        ///     Bad
        /// </summary>
        Bad
    }
}
