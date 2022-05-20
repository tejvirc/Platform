namespace Aristocrat.Monaco.Gaming.VideoLottery.ScreenSaver
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Hardware.Contracts.Cabinet;
    using Kernel;

    [CLSCompliant(false)]
    public partial class ScreenSaverWindow
    {
        private const double SpriteSpeedX = 6;
        private const double SpriteSpeedY = 8;

        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private Vector _vector; 

        public ScreenSaverWindow()
        { 
            InitializeComponent();
            Topmost = true;

            SetWindowSize();
        }
         

        private void ScreensaverWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var rand = new Random(Guid.NewGuid().GetHashCode());

            _vector = new Vector
            {
                X = 1 - rand.NextDouble() * 2,
                Y = 1 - rand.NextDouble() * 2
            };

            _vector.Normalize();

            GameIcon.Width = GameIcon.Source.Width;
            GameIcon.Height = GameIcon.Source.Height;

            CompositionTarget.Rendering += OnUpdate;
        }

        private void ScreensaverWindow_OnUnloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= OnUpdate;
        }

        private void OnUpdate(object sender, object e)
        {
            if (IsOutOfBounds())
            {
                Randomize();
            }

            var sx = _vector.X * SpriteSpeedX;
            var sy = _vector.Y * SpriteSpeedY;

            var x = Canvas.GetLeft(GameIcon) + sx;
            var y = Canvas.GetTop(GameIcon) + sy;

            var srect = new Rect(x, y, GameIcon.Width, GameIcon.Height);
            var drect = new Rect(0, 0, Width, Height);
            if (!drect.Contains(srect))
            {
                if (srect.X < 0 || srect.Right > drect.Width) // Horizontal wall
                {
                    _vector.X *= -1;
                }

                if (srect.Y < 0 || srect.Bottom > drect.Height) // Vertical wall
                {
                    _vector.Y *= -1;
                }
            }

            Canvas.SetLeft(GameIcon, x);
            Canvas.SetTop(GameIcon, y);
        }

        private bool IsOutOfBounds()
        {
            var sx = _vector.X * SpriteSpeedX;
            var sy = _vector.Y * SpriteSpeedY;

            var x = Canvas.GetLeft(GameIcon) + sx;
            var y = Canvas.GetTop(GameIcon) + sy;

            var srect = new Rect(x, y, GameIcon.Width, GameIcon.Height);
            var drect = new Rect(0, 0, Width, Height);

            return !drect.Contains(srect);
        }

        private void Randomize()
        {
            _vector = new Vector
            {
                X = 1 - Random.NextDouble() * 2,
                Y = 1 - Random.NextDouble() * 2
            }; 
        }

        private void SetWindowSize()
        {
            Width = 0;
            Height = 0;
            Left = 0;
            Top = 0;

            var cabinetDetectionService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();

            var monitors = cabinetDetectionService.ExpectedDisplayDevices.Select(
                x => new Rectangle(x.PositionX, x.PositionY, x.Resolution.X, x.Resolution.Y));
            var bounds = monitors.Aggregate(Rectangle.Empty, Rectangle.Union);

            Width = bounds.Width;
            Height = bounds.Height;
            Left = bounds.Left;
            Top = bounds.Top;
        }
    }
}