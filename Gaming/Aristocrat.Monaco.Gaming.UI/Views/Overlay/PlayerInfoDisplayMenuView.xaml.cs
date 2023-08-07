namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using log4net;
    using Monaco.UI.Common;
    using PlayerInfoDisplay;
    using ViewModels;
    using BitmapImage = System.Windows.Media.Imaging.BitmapImage;

    /// <summary>
    ///     Interaction logic for PlayerInfoDisplayMenuView.xaml
    /// </summary>
    public partial class PlayerInfoDisplayMenuView
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public PlayerInfoDisplayMenuView()
        {
            DataContextChanged += (_, __) => Initialize();
            InitializeComponent();
        }

        private void Initialize()
        {
            var viewModel = ViewModel;
            void OnViewModelOnPropertyChanged(object _, PropertyChangedEventArgs eventArgs)
            {
                if (string.CompareOrdinal(ObservablePropertyNames.GameAsset, eventArgs.PropertyName) == 0)
                {
                    Setup();
                }
            }
            if (viewModel == null || viewModel.IsDisabled)
            {
                IsVisibleChanged -= PlayerInfoDisplayMenuView_OnIsVisibleChanged;
                if (viewModel != null)
                {
                    viewModel.PropertyChanged -= OnViewModelOnPropertyChanged;
                }
                return;
            }
            Resources.MergedDictionaries.Add(SkinLoader.Load("PlayerInfoDisplayMenuUI.xaml"));
            IsVisibleChanged += PlayerInfoDisplayMenuView_OnIsVisibleChanged;
            viewModel.PropertyChanged += OnViewModelOnPropertyChanged;
        }

        private bool IsLandscape()
        {
            return Screen.PrimaryScreen.Bounds.Width > Screen.PrimaryScreen.Bounds.Height;
        }

        private ImageBrush GetBackground(PlayerInfoDisplayMenuViewModel viewModel)
        {
            var path = IsLandscape() ? ViewModel.BackgroundImageLandscapePath : viewModel.BackgroundImagePortraitPath;
            ImageBrush brush;
            if (string.IsNullOrWhiteSpace(path))
            {
                Logger.Warn("No locale game background found. Backup background will be used");
                brush = new ImageBrush(
                    IsLandscape()
                        ? (BitmapImage)Resources["Pid_Menu_BackgroundLandscapeImage"]
                        : (BitmapImage)Resources["Pid_Menu_BackgroundPortraitImage"])
                {
                    Stretch = Stretch.None,
                };
            }
            else
            {
                brush = new ImageBrush(new BitmapImage(new Uri(path)))
                {
                    Stretch = Stretch.None,
                };
            }

            return brush;
        }


        private void Setup()
        {
            var viewModel = ViewModel;

            OuterGrid.Background = GetBackground(viewModel);

            if (string.IsNullOrWhiteSpace(viewModel.ExitButtonPath))
            {
                Logger.Warn("No image found for Exit button. Backup background will be used.");
                ExitButton.Style = (Style)Resources["ExitButtonBackupStyle"];
            }

            if (string.IsNullOrWhiteSpace(viewModel.GameInfoButtonPath))
            {
                Logger.Warn("No image found for Game Info button. Backup background will be used.");
                GameInfoButton.Style = (Style)Resources["GameInfoButtonBackupStyle"];
            }

            if (string.IsNullOrWhiteSpace(viewModel.GameRulesButtonPath))
            {
                Logger.Warn("No image found for Game Rules button. Backup background will be used.");
                GameRulesButton.Style = (Style)Resources["GameRulesButtonBackupStyle"];
            }
        }

        private void PlayerInfoDisplayMenuView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ViewModel.IsVisible)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnClickOutside), true);
                Mouse.Capture(this, CaptureMode.SubTree);
            }
            else
            {
                Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnClickOutside);
            }
        }

        private void OnClickOutside(object sender, MouseButtonEventArgs e)
        {
            ViewModel.ExitRequested();
        }

        private PlayerInfoDisplayMenuViewModel ViewModel => (PlayerInfoDisplayMenuViewModel)DataContext;

        private void HandleNonClosingPress(object sender, RoutedEventArgs e)
        {
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnClickOutside);
            AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnClickOutside), true);
            Mouse.Capture(this, CaptureMode.SubTree);
        }
    }
}