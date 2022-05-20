namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System.Windows;
    using System.Windows.Input;
    using Contracts;
    using Monaco.UI.Common;
    using ViewModels;
    using Timer = System.Timers.Timer;
    using BitmapImage = System.Windows.Media.Imaging.BitmapImage;

    /// <summary>
    ///     Interaction logic for PlayerMenuPopupView.xaml
    /// </summary>
    public partial class PlayerMenuPopupView
    {
        private bool _inbetweenTouchs;
        private readonly Timer _inbetweenTouchTimer = new Timer(GamingConstants.PlayerMenuPopupOpenCloseDelayMilliseconds);

        public BitmapImage ThreeSectionBackgroundImage { get; set; }

        public BitmapImage TwoSectionTrackingBackgroundImage { get; set; }

        public BitmapImage TwoSectionReserveBackgroundImage { get; set; }

        public BitmapImage OneSectionBackgroundImage { get; set; }

        public PlayerMenuPopupView()
        {
            InitializeComponent();

            Resources.MergedDictionaries.Add(SkinLoader.Load("PlayerMenuPopupUI.xaml"));
            ThreeSectionBackgroundImage = (BitmapImage)Resources["ThreeSectionBackgroundImage"];
            TwoSectionTrackingBackgroundImage = (BitmapImage)Resources["TwoSectionTrackingBackgroundImage"];
            TwoSectionReserveBackgroundImage = (BitmapImage)Resources["TwoSectionReserveBackgroundImage"];
            OneSectionBackgroundImage = (BitmapImage)Resources["OneSectionBackgroundImage"];

            Root.Width = ThreeSectionBackgroundImage.Width;
            SessionTrackingSection.Height = TwoSectionTrackingBackgroundImage.Height - OneSectionBackgroundImage.Height;
            ReserveMachineSection.Height = TwoSectionReserveBackgroundImage.Height - OneSectionBackgroundImage.Height;
            ButtonSection.Height = OneSectionBackgroundImage.Height;

            _inbetweenTouchTimer.Elapsed += (o, args) => _inbetweenTouchs = false;
        }

        /// <summary>
        ///     Gets or sets the view model for the view.
        /// </summary>
        public PlayerMenuPopupViewModel ViewModel
        {
            get => ((LobbyViewModel)DataContext).PlayerMenuPopupViewModel;
            set => DataContext = value;
        }

        // This functionality is to force some time inbetween handling outside touches.
        // Touch sensitivitly can sometimes cause multiple touches to register, which can close
        // the menu unexpectedly 
        private void SetInbetweenTouchTimer()
        {
            if(_inbetweenTouchTimer.Enabled)
            {
                _inbetweenTouchTimer.Stop();
            }

            _inbetweenTouchs = true;
            _inbetweenTouchTimer.Start();
        }

        private void OnClickOutside(object sender, MouseButtonEventArgs e)
        { 
            if (_inbetweenTouchs)
            {
                return;
            }

            ViewModel.SendButtonPressToExit();

            SetInbetweenTouchTimer();
        }

        private void PlayerMenuPopupView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel.IsMenuVisible)
            {
                SetInbetweenTouchTimer();

                AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnClickOutside), true);
                Mouse.Capture(this, CaptureMode.SubTree);
            }
            else
            {
                Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnClickOutside);
            }
        }

        private void PlayerMenuPopupView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _inbetweenTouchTimer.Dispose();
        }

        private void HandleNonClosingPress(object sender, RoutedEventArgs e)
        {
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnClickOutside);
            AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnClickOutside), true);
            Mouse.Capture(this, CaptureMode.SubTree);
        }
    }
}