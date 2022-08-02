namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System;
    using BitmapImage = System.Windows.Media.Imaging.BitmapImage;
    using Contracts;
    using log4net;
    using Monaco.UI.Common;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using Timer = System.Timers.Timer;
    using ViewModels;

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

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        private void OnUpEventOutside(object sender, MouseButtonEventArgs e)
        {
            SetInBetweenTouchTimer();
            Logger.Debug("Up Event Received and Remove Up Event Handler");
            //Remove handler for touch Up as pop up would be closed for next touch down event
            Mouse.RemovePreviewMouseUpOutsideCapturedElementHandler(this, OnUpEventOutside);

            // Add handler for Touch Down to close the pop up 
            Logger.Debug("Add Handler for Down Event");
            AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnDownEventOutside), true);
            Mouse.Capture(this, CaptureMode.SubTree);
        }

        private void OnDownEventOutside(object sender, MouseButtonEventArgs e)
        {
            Logger.Debug($"Down Event Received with _inBetweenTouches={_inBetweenTouches}");
            if (_inBetweenTouches)
            {
                return;
            }

            Logger.Debug("Down Event Received, Remove Down Event Handler, Send button press exit");
            //Received the touch down event to close the player menu pop up so remove the handler to avoid the other
            // continuous touchs.
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnDownEventOutside);
            ViewModel.SendButtonPressToExit();

            SetInBetweenTouchTimer();
        }

        private void PlayerMenuPopupView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //reset the existing handler if visibilty changes
            Logger.Debug($"Set Visibility {ViewModel.IsMenuVisible} and remove existing handler");
            Mouse.RemovePreviewMouseUpOutsideCapturedElementHandler(this, OnUpEventOutside);
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnDownEventOutside);

            if (ViewModel.IsMenuVisible)
            {
                SetInBetweenTouchTimer();
                Logger.Debug("Set Visibility true and Register Up Event Handler");
                AddHandler(Mouse.PreviewMouseUpOutsideCapturedElementEvent, new MouseButtonEventHandler(OnUpEventOutside), true);
                Mouse.Capture(this, CaptureMode.SubTree);
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
            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(this, OnDownEventOutside);
            AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnDownEventOutside), true);
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