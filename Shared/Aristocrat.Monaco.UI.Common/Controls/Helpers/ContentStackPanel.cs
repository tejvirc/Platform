namespace Aristocrat.Monaco.UI.Common.Controls.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    /// <summary>
    ///     Implementing a scrollable content panel without vertical cutoff for TimeZone comboBox popup box
    /// </summary>
    public class ContentStackPanel : Panel, IScrollInfo
    {
        private const double LineSize = 31.6;
        private const double LineLength = 500;
        private const double WheelSize = 5 * LineSize;
        private const double VerticalMargin1 = 3; //for 480 height of viewport
        private const double VerticalMargin2 = 6.2; //for 360 height of viewport
        private const double ScreenViewport = 480;

        private Size _extent;
        private Vector _offset;
        private Size _viewport;

        private double VerticalMargin => _viewport.Height.Equals(ScreenViewport) ? VerticalMargin1 : VerticalMargin2;

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public ScrollViewer ScrollOwner { get; set; }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public bool CanHorizontallyScroll { get; set; }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public bool CanVerticallyScroll { get; set; }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public double ExtentHeight => _extent.Height - VerticalMargin;

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public double ExtentWidth => _extent.Width;

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public double HorizontalOffset => _offset.X;

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public double VerticalOffset => RoundOffset(_offset.Y);

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public double ViewportHeight => _viewport.Height - 2 * VerticalMargin;

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public double ViewportWidth => _viewport.Width;

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + LineSize);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - LineSize);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - LineSize);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + LineSize);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + WheelSize);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - WheelSize);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - WheelSize);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + WheelSize);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + ViewportWidth);
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (rectangle.IsEmpty || visual == null
                                  || visual == this || !IsAncestorOf(visual))
            {
                return Rect.Empty;
            }

            Debug.WriteLine($"{rectangle.Top},{rectangle.Left},{rectangle.Bottom},{rectangle.Right}");
            rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);

            var viewRect = new Rect(
                HorizontalOffset,
                VerticalOffset,
                ViewportWidth,
                ViewportHeight);
            rectangle.X += viewRect.X;
            rectangle.Y += viewRect.Y;
            viewRect.X = CalculateNewScrollOffset(
                viewRect.Left,
                viewRect.Right,
                rectangle.Left,
                rectangle.Right);
            viewRect.Y = CalculateNewScrollOffset(
                viewRect.Top,
                viewRect.Bottom,
                rectangle.Top,
                rectangle.Bottom);
            SetHorizontalOffset(viewRect.X);
            SetVerticalOffset(viewRect.Y);
            rectangle.Intersect(viewRect);
            rectangle.X -= viewRect.X;
            rectangle.Y -= viewRect.Y;

            return rectangle;
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void SetHorizontalOffset(double offset)
        {
            offset = Math.Max(
                0,
                Math.Min(offset, ExtentWidth - ViewportWidth));
            if (!offset.Equals(_offset.Y))
            {
                _offset.X = offset;
                InvalidateArrange();
            }
        }

        /// <summary>
        ///     declare in IScrollInfo
        /// </summary>
        public void SetVerticalOffset(double offset)
        {
            offset = Math.Max(
                0,
                Math.Min(offset, ExtentHeight - ViewportHeight));
            if (!offset.Equals(_offset.Y))
            {
                _offset.Y = RoundOffset(offset);
                InvalidateArrange();
            }
        }

        /// <summary>
        ///     declare in FrameworkElement
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            double curY = 0, curLineHeight = VerticalMargin, maxLineWidth = 0;
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(LineLength, LineSize));

                curY += curLineHeight;
                double curX = 0;
                curLineHeight = 0;

                curX += child.DesiredSize.Width;
                if (child.DesiredSize.Height > curLineHeight)
                {
                    curLineHeight = child.DesiredSize.Height;
                }

                if (curX > maxLineWidth)
                {
                    maxLineWidth = curX;
                }
            }

            curY += curLineHeight;

            VerifyScrollData(new Size(maxLineWidth, availableSize.Height), new Size(maxLineWidth, curY));

            return _viewport;
        }

        /// <summary>
        ///     declare in FrameworkElement
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children != null && Children.Count == 0)
            {
                return finalSize;
            }

            double curY = 0, curLineHeight = VerticalMargin, maxLineWidth = 0;

            foreach (UIElement child in Children)
            {
                var trans = child.RenderTransform as TranslateTransform;
                if (trans == null)
                {
                    child.RenderTransformOrigin = new Point(0, 0);
                    trans = new TranslateTransform();
                    child.RenderTransform = trans;
                }

                curY += curLineHeight;
                double curX = 0;
                curLineHeight = 0;

                child.Arrange(
                    new Rect(
                        0,
                        0,
                        child.DesiredSize.Width,
                        child.DesiredSize.Height));

                trans.X = curX - HorizontalOffset;
                trans.Y = curY - VerticalOffset;

                curX += child.DesiredSize.Width;
                if (child.DesiredSize.Height > curLineHeight)
                {
                    curLineHeight = child.DesiredSize.Height;
                }

                if (curX > maxLineWidth)
                {
                    maxLineWidth = curX;
                }
            }

            curY += curLineHeight;
            VerifyScrollData(finalSize, new Size(maxLineWidth, curY));

            return finalSize;
        }

        private static double CalculateNewScrollOffset(
            double topView,
            double bottomView,
            double topChild,
            double bottomChild)
        {
            var offBottom = topChild < topView && bottomChild < bottomView;
            var offTop = bottomChild > bottomView && topChild > topView;
            var tooLarge = bottomChild - topChild > bottomView - topView;

            if (!offBottom && !offTop)
            {
                return topView;
            } //Don't do anything, already in view

            if (offBottom && !tooLarge || offTop && tooLarge)
            {
                return topChild;
            }

            return bottomChild - (bottomView - topView);
        }

        private double RoundOffset(double offset)
        {
            var i = (int)(offset * 10.0);
            var ii = (int)(LineSize * 10.0);
            var r = i % ii / 10;
            if (r == 0)
            {
                return offset;
            }

            if (r < LineSize / 2)
            {
                return offset - r;
            }

            return offset - r + LineSize;
        }

        private void VerifyScrollData(Size viewport, Size extent)
        {
            if (double.IsInfinity(viewport.Width))
            {
                viewport.Width = extent.Width;
            }

            if (double.IsInfinity(viewport.Height))
            {
                viewport.Height = extent.Height;
            }

            _extent = extent;
            _viewport = viewport;

            _offset.X = Math.Max(
                0,
                Math.Min(_offset.X, ExtentWidth - ViewportWidth));
            _offset.Y = Math.Max(
                0,
                Math.Min(_offset.Y, ExtentHeight - ViewportHeight));

            ScrollOwner?.InvalidateScrollInfo();
        }
    }
}