namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    /// <summary>
    ///     Defines interface to the inspection wizard.
    /// </summary>
    public interface IInspectionWizard
    {
        /// <summary>
        ///     Set/get whether an automated test can be started.
        /// </summary>
        bool CanStartAutoTest { get; set; }

        /// <summary>
        ///     Set/get the test name text.
        /// </summary>
        string TestNameText { get; set; }
    }
}
