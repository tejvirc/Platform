namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Cabinet.Contracts;

    /// <summary>
    /// </summary>
    [CLSCompliant(false)]
    public partial class TouchScreenTestWindow
    {
        private const int MaxDrawnTouchPoints = 5000;
        private readonly int _firstEllipseIndex;
        private WindowToScreenMapper _mapper;

        public TouchScreenTestWindow()
        {
            InitializeComponent();
            TouchDown += OnTouchDown;
            TouchMove += OnTouchMove;
            Closing += TouchScreenTestWindow_Closing;
            _firstEllipseIndex = TouchCanvas.Children.Count;
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            AddPoint(e.GetTouchPoint(this));
        }

        private void OnTouchMove(object sender, TouchEventArgs e)
        {
            AddPoint(e.GetTouchPoint(this));
        }

        public DisplayRole DisplayRole
        {
            set
            {
                _mapper = new WindowToScreenMapper(value);
                Topmost = _mapper.IsFullscreen;
                _mapper.MapWindow(this);
            }
        }

        public bool Drawable
        {
            set => TouchCanvas.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        }

        private void TouchScreenTestWindow_Closing(object sender, CancelEventArgs e)
        {
            TouchDown -= OnTouchDown;
            TouchMove -= OnTouchMove;
        }

        private void AddPoint(TouchPoint point)
        {
            // Add an Ellipse Element
            var myEllipse = new Ellipse
            {
                Stroke = Brushes.DarkBlue,
                Fill = Brushes.DarkBlue,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 5,
                Height = 5
            };
            Canvas.SetLeft(myEllipse, point.Position.X);
            Canvas.SetTop(myEllipse, point.Position.Y);
            TouchCanvas.Children.Add(myEllipse);
            while (TouchCanvas.Children.Count > _firstEllipseIndex + MaxDrawnTouchPoints)
            {
                TouchCanvas.Children.RemoveAt(_firstEllipseIndex);
            }
        }

        private void OnExitButtonPressed(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}