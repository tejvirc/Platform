namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System.Windows;
    using System.Windows.Input;
    using Monaco.UI.Common;
    using ViewModels;
    using BitmapImage = System.Windows.Media.Imaging.BitmapImage;
    using System.Windows.Media;
    using Kernel;
    using log4net;
    using System.Reflection;

    /// <summary>
    ///     Interaction logic for PlayerMenuPopupView.xaml
    /// </summary>
    public partial class PlayerMenuPopupView
    {
        private int _mouseDownTimeStamp;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
            Logger.Debug($"Visibilty Changed To {ViewModel.IsMenuVisible}");
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
            Logger.Debug($"Touch Up Recieved at {e.Timestamp}");
            var touchPosition = e.GetTouchPoint(this);
            Handle(touchPosition.Position);
        }

        private void Handle(Point point)
        {
            ViewModel.ResetCloseDelay();
            if (InputHitTest(point) is UIElement element && VisualTreeHelper.GetParent(element) is PlayerMenuPopupView)
            {
                UnSubscribeEvents();
                Logger.Debug($"clicked/touched out side player menu");
                ViewModel.SendButtonPressToExit();
            }
            else
            {
                Logger.Debug($"clicked/touched inside player menu");
            }
        }
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            Logger.Debug($"Mouse Up Recieved at {e.Timestamp} and _mouseDownTimeStamp is {_mouseDownTimeStamp}");
            if (_mouseDownTimeStamp == e.Timestamp || _mouseDownTimeStamp == 0)
            {
                Logger.Debug($"This Mouse Up event is ignored");
                return;
            }
            var position = e.GetPosition(this);
            Handle(position);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            Logger.Debug($"Mouse Down Recieved at {e.Timestamp}");
            _mouseDownTimeStamp = e.Timestamp;
        }

        private void SubscribeEvents()
        {
            UnSubscribeEvents();
            var propManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();


            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
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