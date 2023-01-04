namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Kernel;
    using Monaco.UI.Common;
    using Views;

    /// <summary>
    ///     InspectionLauncher is a runnable loaded by the Application Bootstrap. It will facilitate
    ///     loading of the InspectionWindow.
    /// </summary>
    public sealed class InspectionLauncher : BaseRunnable
    {
        private const string WindowName = "InspectionWizard";

        /// <summary>
        ///     The object that shows the window in a properly configured thread.
        /// </summary>
        private IWpfWindowLauncher _windowLauncher;

        /// <summary>
        ///     Empty Initialize function.
        /// </summary>
        protected override void OnInitialize()
        {
            _windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
        }

        /// <summary>
        ///     Displays the window and blocks until it is closed.
        /// </summary>
        protected override void OnRun()
        {
            // This blocks until the dialog is closed.
            _windowLauncher.CreateWindow<InspectionWindow>(WindowName, true);
        }

        /// <summary>
        ///     Closes the window.
        /// </summary>
        protected override void OnStop()
        {
            _windowLauncher.Close(WindowName);
        }
    }
}