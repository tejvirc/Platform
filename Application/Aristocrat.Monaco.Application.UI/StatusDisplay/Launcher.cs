namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using Kernel;
    using log4net;
    using Monaco.UI.Common;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    ///     Definition of the Launcher class.
    /// </summary>
    public class Launcher : BaseRunnable, IService, IPlatformDisplay
    {
        private const string WindowName = "StatusWindow";
        private readonly IWpfWindowLauncher _windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();

        /// <inheritdoc />
        public bool IsVisible => _windowLauncher?.GetWindowVisibility(WindowName) == System.Windows.Visibility.Visible;
      
        /// <inheritdoc />
        public void CreateAndShow()
        {
            var windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
            windowLauncher.CreateWindow<StatusDisplayView>(WindowName);
        }

        /// <inheritdoc />
        public void Close()
        {
            var windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
            windowLauncher.Close(WindowName);
        }

        /// <inheritdoc />
        public void Show()
        {
            var windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
            windowLauncher.Show(WindowName);
        }

        /// <inheritdoc />
        public void Hide()
        {
            var windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
            windowLauncher.Hide(WindowName);
        }

        /// <inheritdoc />
        public void Shutdown(bool closeApplication)
        {
            var windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
            Close();
            if (closeApplication)
            {
                // This effectively controls the Application instance.
                // We need to explicity close the application at exit, but in the event of a soft reboot (due to a RAM clear, etc.)
                //   we can't shut it down or we risk failiing due to trying to create more than one application in the current app domain.
                windowLauncher.Shutdown();
            }
        }

        /// <inheritdoc />
        public string Name => "StatusDisplay";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPlatformDisplay) };

        /// <inheritdoc />
        protected override void OnInitialize()
        {
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            CreateAndShow();
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
        }
    }
}