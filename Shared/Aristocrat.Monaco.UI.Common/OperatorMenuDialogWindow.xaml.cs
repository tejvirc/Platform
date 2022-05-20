namespace Aristocrat.Monaco.UI.Common
{
    using System;

    /// <summary>
    /// OperatorMenuDialogWindow is a full-screen transparent wrapper for our modal dialogs
    /// to get around a WPF touch bug.
    /// As of August 2019, with WPF, if you have a modal dialog up via ShowDialog
    /// and the user touches repeatedly outside of the modal dialog, the touch
    /// interface will lock up, sometimes for up to 30 seconds
    /// By making our dialogs full-screen but transparent (.01 opacity) we can
    /// capture all of the clicks "outside" of the modal dialog on the same window
    /// as the actual dialog, thus working around the WPF bug.
    /// </summary>
    public partial class OperatorMenuDialogWindow : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// OperatorMenuDialogWindow
        /// </summary>
        public OperatorMenuDialogWindow(object content)
        {
            InitializeComponent();
            DialogContent.Content = content;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes of resources
        /// </summary>
        /// <param name="disposing">true if disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (DialogContent != null && DialogContent is IDisposable content)
                {
                    content.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
