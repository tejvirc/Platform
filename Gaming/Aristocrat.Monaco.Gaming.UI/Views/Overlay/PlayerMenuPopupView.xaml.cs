namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System;
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
    public partial class PlayerMenuPopupView : IDisposable
    {
        private bool _inBetweenTouches;
        private readonly Timer _inBetweenTouchTimer = new Timer(GamingConstants.PlayerMenuPopupOpenCloseDelayMilliseconds);
        private bool _disposed;

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

            _inBetweenTouchTimer.Elapsed += (o, args) => _inBetweenTouches = false;
        }

        /// <summary>
        ///     Gets or sets the view model for the view.
        /// </summary>
        public PlayerMenuPopupViewModel ViewModel
        {
            get => ((LobbyViewModel)DataContext).PlayerMenuPopupViewModel;
            set => DataContext = value;
        }

        // This functionality is to force some time in between handling outside touches.
        // Touch sensitivity can sometimes cause multiple touches to register, which can close
        // the menu unexpectedly 
        private void SetInBetweenTouchTimer()
        {
            if (_inBetweenTouchTimer.Enabled)
            {
                _inBetweenTouchTimer.Stop();
            }

            _inBetweenTouches = true;
            _inBetweenTouchTimer.Start();
        }

        private void OnClickOutside(object sender, MouseButtonEventArgs e)
        { 
            if (_inBetweenTouches)
            {
                return;
            }

            ViewModel.SendButtonPressToExit();

            SetInBetweenTouchTimer();
        }

        private void PlayerMenuPopupView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel.IsMenuVisible)
            {
                SetInBetweenTouchTimer();

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
            _inBetweenTouchTimer.Dispose();
        }

        private void HandleNonClosingPress(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetCloseDelay();
            ViewModel.PlayClickSound();
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnClickOutside);
            AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnClickOutside), true);
            Mouse.Capture(this, CaptureMode.SubTree);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                return;
            }

            if (disposing)
            {
                _inBetweenTouchTimer.Dispose();
            }

            _disposed = true;
        }
    }
}