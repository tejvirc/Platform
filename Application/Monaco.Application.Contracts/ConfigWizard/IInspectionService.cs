namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    using System.Collections.Generic;
    using HardwareDiagnostics;
    using Kernel;
    using OperatorMenu;

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
        ///     Set up the current page loader.
        /// </summary>
        /// <param name="loader">Current page loader</param>

        HardwareDiagnosticDeviceCategory SetCurrentPageLoader(IOperatorMenuPageLoader loader);

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
}
