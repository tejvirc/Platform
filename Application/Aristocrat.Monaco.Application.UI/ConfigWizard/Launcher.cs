namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Kernel;
    using Monaco.UI.Common;
    using Views;

    /// <summary>
    ///     Launcher is a runnable loaded by the ApplicationBootstrap. It will facilitate
    ///     loading of the StaLauncher and ConfigSelectionWindow.
    /// </summary>
    public sealed class Launcher : BaseRunnable
    {
        private const string WindowName = "ConfigWizard";

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
            _windowLauncher.CreateWindow<SelectionWindow>(WindowName, true);
        }

        /// <summary>
        ///     Closes the window.
        /// </summary>
        protected override void OnStop()
        {
            _windowLauncher.Close(WindowName);
        }

        /// <summary>
        ///     Disposes of resources that are no longer needed
        /// </summary>
        /// <param name="disposing">Whether or not the object is being disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    _windowLauncher.Dispose();
                }
            }
        }
    }
}