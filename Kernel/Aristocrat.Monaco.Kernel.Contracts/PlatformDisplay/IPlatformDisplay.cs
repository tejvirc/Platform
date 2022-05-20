namespace Aristocrat.Monaco.Kernel
{
    /// <summary>
    ///     Defines an interface that the Display components of the platform should implement.
    /// </summary>
    public interface IPlatformDisplay
    {
        /// <summary>
        ///     Creates and shows the IPlatformDisplay to the user.
        /// </summary>
        void CreateAndShow();

        /// <summary>
        ///     Closes the IPlatformDisplay to the user.
        /// </summary>
        void Close();

        /// <summary>
        ///     Shows the IPlatformDisplay from the user.
        /// </summary>
        void Show();

        /// <summary>
        ///     Hides the IPlatformDisplay from the user.
        /// </summary>
        void Hide();

        /// <summary>
        ///     Shuts down the presentation layer
        /// </summary>
        /// <param name="closeApplication">If true the main application loop will be closed</param>
        void Shutdown(bool closeApplication);
    }
}