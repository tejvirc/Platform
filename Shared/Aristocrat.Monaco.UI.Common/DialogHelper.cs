namespace Aristocrat.Monaco.UI.Common
{
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using Application.Contracts.OperatorMenu;
    using log4net;

    /// <summary>
    /// Helper class for IDialogService
    /// </summary>
    public static class DialogHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// ShowDialog creates a modal dialog out of the passed in window and viewmodel.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static bool? ShowDialog(Window dialog, IModalDialogViewModel viewModel)
        {
            var handler = RegisterDialogResult(dialog, viewModel);
            dialog.Loaded += (sender, args) =>
            {
                WpfWindowLauncher.DisableStylus(dialog);
            };

            dialog.ShowDialog();

            UnregisterDialogResult(viewModel, handler);

            return viewModel.DialogResult;
        }

        private static PropertyChangedEventHandler RegisterDialogResult(Window dialog, IModalDialogViewModel viewModel)
        {
            void Handler(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(viewModel.DialogResult) &&
                    dialog.DialogResult != viewModel.DialogResult)
                {
                    dialog.DialogResult = viewModel.DialogResult;
                }
            }

            viewModel.PropertyChanged += Handler;

            return Handler;
        }

        private static void UnregisterDialogResult(
            INotifyPropertyChanged viewModel,
            PropertyChangedEventHandler handler)
        {
            viewModel.PropertyChanged -= handler;
        }

        /// <summary>
        /// FindOwner dialog helper function
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static Window FindOwner(INotifyPropertyChanged viewModel)
        {
            var view = DialogParticipation.GetAssociation(viewModel);

            if (view == null)
            {
                Logger.Error("Context is not registered. Consider using DialogParticipation. Register in XAML to bind in the DataContext.");
                return null;
            }

            // Get owner window
            var owner = Window.GetWindow(view);
            if (owner == null)
            {
                Logger.Error($"View of type '{view.GetType()}' is not registered.");
            }

            return owner;
        }
    }
}
