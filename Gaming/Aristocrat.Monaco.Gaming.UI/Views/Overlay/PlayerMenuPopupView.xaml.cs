namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System.Windows;
    using System.Windows.Input;
    using Monaco.UI.Common;
    using ViewModels;
    using BitmapImage = System.Windows.Media.Imaging.BitmapImage;
    using System.Windows.Media;
    using Kernel;

    /// <summary>
    ///     Interaction logic for PlayerMenuPopupView.xaml
    /// </summary>
    public partial class PlayerMenuPopupView
    {
        private int _mouseDownTimeStamp;
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
            _mouseDownTimeStamp = 0;
        }

        /// <summary>
        ///     Gets or sets the view model for the view.
        /// </summary>
        public PlayerMenuPopupViewModel ViewModel
        {
            get => ((LobbyViewModel)DataContext).PlayerMenuPopupViewModel;
            set => DataContext = value;
        }

        private void PlayerMenuPopupView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _mouseDownTimeStamp = 0;
            if (ViewModel.IsMenuVisible)
            {
                SubscribeEvents();
            }
            else
            {
                UnSubscribeEvents();
            }
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            var touchPosition = e.GetTouchPoint(this);
            Handle(touchPosition.Position);
        }

        private void Handle(Point point)
        {
            ViewModel.ResetCloseDelay();
            if (InputHitTest(point) is UIElement element && VisualTreeHelper.GetParent(element) is PlayerMenuPopupView)
            {
                UnSubscribeEvents();
                ViewModel.SendButtonPressToExit();
            }
        }
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (_mouseDownTimeStamp == e.Timestamp || _mouseDownTimeStamp == 0)
            {
                return;
            }
            var position = e.GetPosition(this);
            Handle(position);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            _mouseDownTimeStamp = e.Timestamp;
        }

        private void SubscribeEvents()
        {
            UnSubscribeEvents();
            var propManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            if (propManager.GetValue("display", string.Empty) == "windowed")
            {
                MouseDown += OnMouseDown;
                MouseUp += OnMouseUp;
            }
            TouchUp += OnTouchUp;
        }

        private void UnSubscribeEvents()
        {
            MouseDown -= OnMouseDown;
            MouseUp -= OnMouseUp;
            TouchUp -= OnTouchUp;
        }
        private void HandleNonClosingPress(object sender, RoutedEventArgs e)
        {
            SubscribeEvents();
            ViewModel.PlayClickSound();
        }
    }
}