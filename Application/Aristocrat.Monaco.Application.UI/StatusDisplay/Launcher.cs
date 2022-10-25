namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using Kernel;
    using Monaco.UI.Common;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the Launcher class.
    /// </summary>
    public class Launcher : BaseRunnable, IService, IPlatformDisplay
    {
        private const string WindowName = "StatusWindow";

        /// <inheritdoc />
        public void CreateAndShow()
        {
            ServiceManager.GetInstance().GetService<IWpfWindowLauncher>().CreateWindow<StatusDisplayView>(WindowName);
        }

        /// <inheritdoc />
        public void Close()
        {
            ServiceManager.GetInstance().GetService<IWpfWindowLauncher>().Close(WindowName);
        }

        /// <inheritdoc />
        public void Show()
        {
            ServiceManager.GetInstance().GetService<IWpfWindowLauncher>().Show(WindowName);
        }

        /// <inheritdoc />
        public void Hide()
        {
            ServiceManager.GetInstance().GetService<IWpfWindowLauncher>().Hide(WindowName);
        }

        /// <inheritdoc />
        public void Shutdown(bool closeApplication)
        {
            Close();
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