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
        ///     Set a reference to the inspection wizard.
        /// </summary>
        /// <param name="wizard">That man behind the curtain.</param>
        void SetWizard(IInspectionWizard wizard);

        /// <summary>
        ///     Manually start the automated test.
        /// </summary>
        void ManuallyStartAutoTest();

        /// <summary>
        ///     Set up the following test's name.
        /// </summary>
        /// <param name="testName">Test name</param>
        void SetTestName(string testName);

        /// <summary>
        ///     Report a test failure.
        /// </summary>
        /// /// <param name="failureMessage">User input failure message.</param>
        void ReportTestFailure(string failureMessage = "");
    }
}
