namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Ink;
    using System.Windows.Input;
    using System.Windows.Media;
    using Cabinet.Contracts;
    using Contracts;
    using Kernel;

    /// <summary>
    /// </summary>
    [CLSCompliant(false)]
    public partial class TouchScreenTestWindow
    {
        private WindowToScreenMapper _mapper;
        private static int _maxTouchPointsPerStroke = 10;
        private static int _maxStrokesCount = 500;
        private static double _minNearbyDistance = 0.5;

        private Dictionary<int, DottedStroke> _strokes = new Dictionary<int, DottedStroke>();

        public TouchScreenTestWindow()
        {
            InitializeComponent();

            TouchDown += OnTouchDown;
            TouchMove += OnTouchMove;
            TouchUp += OnTouchUp;
            Closing += TouchScreenTestWindow_Closing;
            TouchCanvas.UseCustomCursor = true;
            TouchCanvas.Cursor = Cursors.Arrow;
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            var touchPoint = e.GetTouchPoint(this);
            AddNewStroke(touchPoint);
        }

        private void OnTouchMove(object sender, TouchEventArgs e)
        {
            var touchPoint = e.GetTouchPoint(this);
            var point = touchPoint.Position;

            if (_strokes.ContainsKey(touchPoint.TouchDevice.Id))
            {
                var stroke = _strokes[touchPoint.TouchDevice.Id];
                var nearbyPoint = CheckPointNearby(stroke, point);

                // Create another stroke if points count exceeds some certain number for the performance purpose
                if (stroke.StylusPoints.Count > _maxTouchPointsPerStroke)
                {

                    while (TouchCanvas.Strokes.Count > _maxStrokesCount) TouchCanvas.Strokes.RemoveAt(0);

                    var lastDrawnPoint = stroke.LastDrawnPoint;
                    _strokes.Remove(touchPoint.TouchDevice.Id);

                    stroke = AddNewStroke(touchPoint, lastDrawnPoint);
                }

                if (!nearbyPoint)
                {
                    stroke.StylusPoints.Add(new StylusPoint(point.X, point.Y));
                }
            }
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            var touchPoint = e.GetTouchPoint(this);
            if (_strokes.ContainsKey(touchPoint.TouchDevice.Id))
                _strokes.Remove(touchPoint.TouchDevice.Id);
        }

        public DisplayRole DisplayRole
        {
            set
            {
                _mapper = new WindowToScreenMapper(value);
                Topmost = ServiceManager.GetInstance().GetService<IPropertiesManager>().IsFullScreen();
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
            TouchUp -= OnTouchUp;
        }

        private DottedStroke AddNewStroke(TouchPoint touchPoint, Point? lastDrawnPoint = null)
        {
            var point = touchPoint.Position;
            var stroke = new DottedStroke(new StylusPointCollection(new List<Point> { point }), lastDrawnPoint);
            if (!_strokes.ContainsKey(touchPoint.TouchDevice.Id))
            {
                _strokes.Add(touchPoint.TouchDevice.Id, stroke);
                TouchCanvas.Strokes.Add(stroke);
            }
            return stroke;
        }

        private static bool CheckPointNearby(Stroke stroke, Point point)
        {
            var last = stroke.StylusPoints.LastOrDefault();     // starting search from the end should be faster
            return (Math.Abs(last.X - point.X) <= _minNearbyDistance) && (Math.Abs(last.Y - point.Y) <= _minNearbyDistance);
        }

        private void OnExitButtonPressed(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    internal class DottedStroke : Stroke
    {
        private readonly Pen _pen;
        private static double _minDistanceBetweenPoints = 6;
        private readonly Point? _initialPoint;

        public DottedStroke(StylusPointCollection stylusPoints, Point? initialPoint = null)
            : base(stylusPoints)
        {
            _initialPoint = initialPoint;
            LastDrawnPoint = initialPoint;
            _pen = new Pen(Brushes.DarkBlue, 1);
        }

        public Point? LastDrawnPoint { get; private set; }

        protected override void DrawCore(DrawingContext drawingContext,
                                         DrawingAttributes drawingAttributes)
        {
            var prevPoint = _initialPoint ?? new Point(double.NegativeInfinity,
                                        double.NegativeInfinity);

            for (var i = 0; i < StylusPoints.Count; i++)
            {
                var pt = (Point)StylusPoints[i];
                var vector = Point.Subtract(prevPoint, pt);

                // Only draw if we are at least 6 units away from the end of the last point.
                if (vector.Length > _minDistanceBetweenPoints)
                {
                    var radius = 2.5d;
                    drawingContext.DrawEllipse(Brushes.DarkBlue, _pen, pt, radius, radius);
                    prevPoint = pt;
                    LastDrawnPoint = pt;
                }
            }
        }
    }
}