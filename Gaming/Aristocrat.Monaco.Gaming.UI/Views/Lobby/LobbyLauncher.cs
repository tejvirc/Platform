namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System;
    using Contracts.Lobby;
    using Kernel;
    using Monaco.UI.Common;

    /// <summary>
    ///     The lobby is the main driver for Video Lottery Games.  It is responsible for displaying the enabled game combos
    ///     (theme, paytable, denom).
    /// </summary>
    public class LobbyLauncher : ILobby, IDisposable
    {

        private const string StatusWindowName = "StatusWindow";
        private const string LobbyWindowName = "LobbyWindow";

        private bool _disposed;
        private IWpfWindowLauncher _windowLauncher;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyLauncher" /> class.
        /// </summary>
        public LobbyLauncher()
        {
            _windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void CreateWindow()
        {
            _windowLauncher.CreateWindow<LobbyView>(LobbyWindowName);
            _windowLauncher.Hide(StatusWindowName);
        }

        /// <inheritdoc />
        public void Show()
        {
            _windowLauncher.Show(LobbyWindowName);
        }

        /// <inheritdoc />
        public void Hide()
        {
            _windowLauncher.Hide(LobbyWindowName);
        }

        /// <inheritdoc />
        public void Close()
        {
            _windowLauncher.Show(StatusWindowName);
            _windowLauncher.Close(LobbyWindowName);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _windowLauncher = null;

            _disposed = true;
        }
    }
}
