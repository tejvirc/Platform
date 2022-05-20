namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;

    /// <summary>
    /// Interaction logic for OperatorMenuDialog.xaml
    /// </summary>
    public partial class OperatorMenuDialog : IDisposable
    {
        private bool _disposed;

        public OperatorMenuDialog(object content)
        {
            InitializeComponent();
            DialogContent.Content = content;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (DataContext is IDisposable vm)
                {
                    vm.Dispose();
                }

                if (DialogContent != null && DialogContent is IDisposable content)
                {
                    content.Dispose();
                }

                Resources.MergedDictionaries.Clear();
                Resources.Clear();
            }

            _disposed = true;
        }
    }
}
