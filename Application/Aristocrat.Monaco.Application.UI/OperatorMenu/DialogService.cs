namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;
    using Monaco.UI.Common.Events;

    /// <summary>
    ///     Implementation of the Dialog service.
    /// </summary>
    [CLSCompliant(false)]
    public class DialogService : IDialogService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static IEventBus _eventBus;
        private readonly object _lock = new object();
        private bool _isDialogOpen;

        public string Name => GetType().ToString();

        public bool IsDialogOpen
        {
            get
            {
                lock (_lock)
                {
                    return _isDialogOpen;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _isDialogOpen = value;
                }
            }
        }

        public ICollection<Type> ServiceTypes => new[] { typeof(IDialogService) };

        public void Initialize()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        }

        public bool? ShowDialog<T>(
            INotifyPropertyChanged ownerViewModel,
            IModalDialogSaveViewModel viewModel,
            string windowTitle,
            DialogButton buttons = DialogButton.Save | DialogButton.Cancel,
            IEnumerable<IDialogButtonCustomTextItem> customText = null,
            string windowInfoText = null)
            where T : IOperatorMenuPage
        {
            if (ownerViewModel == null)
            {
                throw new ArgumentNullException(nameof(ownerViewModel));
            }

            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            return CreateAndShowDialog(
                typeof(T),
                ownerViewModel,
                viewModel,
                windowTitle,
                buttons,
                customText,
                windowInfoText);
        }

        public bool? ShowInfoDialog<T>(
            INotifyPropertyChanged ownerViewModel,
            IModalDialogSaveViewModel viewModel,
            string windowTitle)
            where T : IOperatorMenuPage
        {
            return ShowDialog<T>(ownerViewModel,
                viewModel,
                windowTitle,
                DialogButton.Cancel,
                new DialogButtonCustomText
                {
                    { DialogButton.Cancel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Close) }
                });
        }

        public bool? ShowYesNoDialog(INotifyPropertyChanged ownerViewModel, string windowTitle, string windowInfoText = null)
        {
            return ShowDialog(ownerViewModel,
                windowTitle,
                DialogButton.Save | DialogButton.Cancel,
                new DialogButtonCustomText
                {
                    { DialogButton.Save, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Yes) },
                    { DialogButton.Cancel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.No) }
                },
                windowInfoText);
        }

        public bool? ShowDialog(
            INotifyPropertyChanged ownerViewModel,
            string windowTitle,
            DialogButton buttons,
            IEnumerable<IDialogButtonCustomTextItem> customText,
            string windowInfoText = null)
        {
            if (ownerViewModel == null)
            {
                throw new ArgumentNullException(nameof(ownerViewModel));
            }

            return CreateAndShowDialog(
                null,
                ownerViewModel,
                null,
                windowTitle,
                buttons,
                customText,
                windowInfoText);
        }

        public bool DismissOpenedDialog()
        {
            // This is specifically to dismiss OperatorMenuDialogWindow since
            // it is the only overlay dialog window type we create according to CreateAndShowDialog method
            return DismissOpenedDialog<OperatorMenuDialogWindow>();
        }

        private bool DismissOpenedDialog<T>() where T : Window
        {
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
            {
                var openedDialog = Application.Current.Windows.OfType<T>().FirstOrDefault();

                if (openedDialog != null && openedDialog.DialogResult == null)
                {
                    openedDialog.DialogResult = false;
                    return true;
                }
            }

            return false;
        }

        private bool? CreateAndShowDialog(
            Type dialogType,
            INotifyPropertyChanged ownerViewModel,
            IModalDialogSaveViewModel viewModel,
            string windowTitle,
            DialogButton buttons,
            IEnumerable<IDialogButtonCustomTextItem> customButtonText,
            string windowInfoText = null)
        {
            lock (_lock)
            {
                if (IsDialogOpen)
                {
                    // The background UI will not receive user input until this modal dialog is closed.
                    // This is a Windows OS concept that relates to how Modal Dialogs work in Windows.
                    Logger.Error("Cannot open a new dialog while an existing dialog is open. It must be closed first.");
                    return null;
                }

                IsDialogOpen = true;
            }

            UserControl internalView = null;
            if (dialogType != null)
            {
                internalView = Activator.CreateInstance(dialogType) as UserControl;
                if (internalView != null)
                {
                    internalView.DataContext = viewModel;
                }
            }

            var operatorMenuDialog = new OperatorMenuDialog(internalView);
            var operatorMenuViewModel = viewModel != null
                ? new OperatorMenuDialogViewModel(viewModel, windowTitle, buttons, customButtonText, windowInfoText)
                : new OperatorMenuDialogViewModel(windowTitle, buttons, customButtonText, windowInfoText);

            operatorMenuDialog.DataContext = operatorMenuViewModel;
            var owner = DialogHelper.FindOwner(ownerViewModel);

            if (owner == null)
            {
                Logger.Error("CreateAndShowDialog: Owner window is null and dialog will not be created.");
                return null;
            }

            Logger.Debug("CreateAndShowDialog: " +
                         $"OwnerViewModel={ownerViewModel?.GetType().Name}, " +
                         $"OwnerView={owner.GetType().Name}, " +
                         $"ViewModel={viewModel?.GetType().Name}, " +
                         $"View={internalView?.GetType().Name}");

            Window dialog = new OperatorMenuDialogWindow(operatorMenuDialog)
            {
                Owner = owner,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            dialog.SizeChanged += (_, _) => CenterDialogOnElement(owner, dialog);
            _eventBus.Publish(new DialogOpenedEvent());

            bool? result;
            try
            {
                result = DialogHelper.ShowDialog(dialog, operatorMenuViewModel);
            }
            finally
            {
                lock (_lock)
                {
                    IsDialogOpen = false;
                }

                if (internalView is IDisposable view)
                {
                    view.Dispose();
                }

                _eventBus.Publish(new DialogClosedEvent());
            }

            return result;
        }

        private static void CenterDialogOnElement(FrameworkElement owner, Window dialog)
        {
            var source = PresentationSource.FromVisual(owner);
            if (source?.CompositionTarget != null && owner is BaseWindow window)
            {
                CenterDialogOnElement(source.CompositionTarget, window.ContentElement, dialog);
            }
        }

        private static void CenterDialogOnElement(CompositionTarget target, FrameworkElement element, Window dialog)
        {
            if (element == null)
            {
                return;
            }

            // Get upper left corner of the AuditMenu content grid
            var locationFromScreen = element.PointToScreen(new Point(0, 0));

            // Transform screen point to WPF device independent point
            var targetPoint = target.TransformFromDevice.Transform(locationFromScreen);

            // Get scale used to compensate for high DPI screens
            var contentPresenter = TreeHelper.FindChild<ContentPresenter>(dialog);
            var scale = (contentPresenter?.LayoutTransform as ScaleTransform)?.ScaleX ?? 1.0;

            // Get center point of the AuditMenu content grid
            var focus = new Point
            {
                X = targetPoint.X + element.ActualWidth * scale / 2.0,
                Y = targetPoint.Y + element.ActualHeight * scale / 2.0
            };
            Logger.Debug($"Audit menu content width={element.ActualWidth}, height={element.ActualHeight}, top left point=({targetPoint.X}, {targetPoint.Y}), center focus point=({focus.X}, {focus.Y})");

            // Place dialog in center of AuditMenu content grid
            dialog.WindowStartupLocation = WindowStartupLocation.Manual;
            dialog.Top = focus.Y - dialog.ActualHeight / 2;
            dialog.Left = focus.X - dialog.ActualWidth / 2;

            Logger.Debug($"Dialog width={dialog.ActualWidth}, height={dialog.ActualHeight}, top-left point=({dialog.Left}, {dialog.Top})");
        }
    }
}