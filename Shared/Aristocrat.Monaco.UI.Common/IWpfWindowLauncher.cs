namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Windows;
    using Kernel;

    /// <summary>
    ///     Interface for window service.  The window service maintains the UI thread and the collection
    ///     of all windows created through it.  Windows are referenced by name, and the interface provides
    ///     limited support for manipulating.
    /// </summary>
    public interface IWpfWindowLauncher : IService, IDisposable
    {
        /// <summary>
        ///     Creates the window.
        /// </summary>
        /// <typeparam name="T">The type of window to show</typeparam>
        /// <param name="name">The name by which to refer to the window.</param>
        /// <param name="isDialog">True if the window should be shown as a modal dialog.</param>
        void CreateWindow<T>(string name, bool isDialog = false)
            where T : Window, new();

        /// <summary>
        ///     Returns the Window with the specified name.
        ///     WARNING: Any UI changes to the window needs to be done on the UI thread using Dispatcher.Invoke.
        ///     Use the methods of this interface when possible instead of working directly with the Window.
        /// </summary>
        /// <param name="name">The name of the window to get.</param>
        /// <returns>The window or null if the window is not found.</returns>
        Window GetWindow(string name);

        /// <summary>
        ///     Closes the window.
        /// </summary>
        /// <param name="name">The name of the window to close.</param>
        void Close(string name);

        /// <summary>
        ///     Shows the window.
        /// </summary>
        /// <param name="name">The name of the window to show.</param>
        /// <param name="topmost">Dealing with the topmost (Audit Menu) window.</param>
        void Show(string name, bool topmost = false);

        /// <summary>
        ///     Hides the window.
        /// </summary>
        /// <param name="name">The name of the window to hide.</param>
        /// <param name="topmost">Dealing with the topmost (Audit Menu) window.</param>
        void Hide(string name, bool topmost = false);

        /// <summary>
        ///     Activates the window.
        /// </summary>
        /// <param name="name">The name of the window to activate.</param>
        void Activate(string name);

        /// <summary>
        ///     Gets the window visibility.
        /// </summary>
        /// <param name="name">The name of the window to check visibility on.</param>
        /// <returns>The window visibility.</returns>
        Visibility GetWindowVisibility(string name);

        /// <summary>
        ///     Gets the window state.
        /// </summary>
        /// <param name="name">The name of the window to check state on.</param>
        /// <returns>The state of the window.</returns>
        WindowState GetWindowState(string name);

        /// <summary>
        ///     Sets the window state.
        /// </summary>
        /// <param name="name">The name of the window to set state on.</param>
        /// <param name="state">The state of the window.</param>
        void SetWindowState(string name, WindowState state);

        /// <summary>
        ///     Explicitly closes all windows except for the <see cref="Application.MainWindow"/>.
        /// </summary>
        void CloseAllWindows();
    }
}