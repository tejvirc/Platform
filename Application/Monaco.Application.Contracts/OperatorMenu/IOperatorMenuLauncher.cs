namespace Vgt.Client12.Application.OperatorMenu
{
    using System;

    /// <summary>
    ///     An interface to expose the status of the operator launcher and commands which
    ///     can be used to send requests to the launcher.
    /// </summary>
    /// <remarks>
    ///     This is a service interface. The main duty of service implementing this interface
    ///     is to manage the operator menu like showing or hiding it, and to report the operator
    ///     menu's status, e.g, whether it is showing up.
    /// </remarks>
    public interface IOperatorMenuLauncher
    {
        /// <summary>
        ///     Gets a value indicating whether the operator menu is currently showing
        /// </summary>
        bool IsShowing { get; }

        /// <summary>
        ///     Gets a value indicating whether the operator key is disabled.
        /// </summary>
        bool IsOperatorKeyDisabled { get; }

        /// <summary>
        ///     Instructs the implementation to ignore operator key usage.
        /// </summary>
        /// <param name="enabler">The object that can re-enable key usage.</param>
        /// <remarks>
        ///     When it is called, switching operator key will be ignored, meaning the
        ///     operator menu won't be displayed.
        /// </remarks>
        void DisableKey(Guid enabler);

        /// <summary>
        ///     Instructs the implementation to begin/resume operator key usage.
        /// </summary>
        /// <param name="enabler">The object enabling the functionality.</param>
        /// <remarks>
        ///     This method sends a request to the launcher to make the operator key work
        ///     for the <paramref name="enabler" />. But The operator key won't work until no
        ///     <c>DisableKey</c> request is pending.
        /// </remarks>
        void EnableKey(Guid enabler);

        /// <summary>
        ///     API for turning the operator key.
        /// </summary>
        void TurnOperatorKey();

        /// <summary>
        ///     Command to display the operator menu now.
        /// </summary>
        /// <returns>Whether or not the operator menu was shown.</returns>
        bool Show();

        /// <summary>
        ///     Hides the operator menu.  The main motivation for this is replay which needs to temporarily hide
        ///     the operator menu to show the replay, but then return to the operator menu.  We do not want to
        ///     close the operator menu in replay, but keeping the window open is causing windowing order bugs.
        /// </summary>
        void Hide();

        /// <summary>
        ///     Command to take down the operator menu now.
        /// </summary>
        void Close();

        /// <summary>
        ///     Command to activate (bring to front) the operator menu now.
        /// </summary>
        void Activate();

        /// <summary>
        /// Sets a flag to prevent the operator menu from exiting
        /// </summary>
        void PreventExit();

        /// <summary>
        /// Clears the flag that prevents the operator menu from exiting
        /// </summary>
        void AllowExit();
    }
}