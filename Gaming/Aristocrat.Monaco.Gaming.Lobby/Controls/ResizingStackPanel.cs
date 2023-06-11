namespace Aristocrat.Monaco.Gaming.Lobby.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class ResizingStackPanel : StackPanel
    {
        /// <summary>
        ///     DependencyProperty for Padding
        /// </summary>
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(ResizingStackPanel),
                new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        ///     Give this stack panel the ability to pad its contents
        /// </summary>
        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);

            set => SetValue(PaddingProperty, value);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Orientation == Orientation.Horizontal)
            {
                ResizeHorizontally(arrangeSize);
            }
            else
            {
                ResizeVertically(arrangeSize);
            }

            return arrangeSize;
        }

        private void ResizeHorizontally(Size arrangeSize)
        {
            if (Width == 0)
            {
                return;
            }

            // Get the total width of all the children combined before scaling
            var childrenWidth = (from UIElement child in InternalChildren where child != null select child.DesiredSize.Width).Sum();

            // Adjust the max allowed width to account for any padding
            var maxAllowedWidth = Width - Padding.Left - Padding.Right;

            // If the childrenWidth is greater than the maxAllowedWidth, scale down
            var scalePct = maxAllowedWidth > 0 && childrenWidth > maxAllowedWidth
                ? maxAllowedWidth / childrenWidth
                : 1.0d;

            // Shift the starting position of the content according to the alignment choice
            var amountPerSideToCenter = Math.Max(0, Width - childrenWidth) / 2.0;
            var rcChild = new Rect(arrangeSize);
            var previousChildSize = 0.0d;
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    previousChildSize = Math.Max(0, Padding.Left);
                    break;
                case HorizontalAlignment.Stretch:
                case HorizontalAlignment.Center:
                    previousChildSize = Math.Max(Padding.Left, amountPerSideToCenter);
                    break;
                case HorizontalAlignment.Right:
                    previousChildSize = Math.Min(Width - childrenWidth, Width - (childrenWidth + Padding.Right));
                    break;
            }

            // Apply the scaling and position the items
            foreach (UIElement child in InternalChildren)
            {
                if (child == null)
                {
                    continue;
                }

                var myScaleTransform = new ScaleTransform { ScaleY = 1.0, ScaleX = scalePct };
                child.RenderTransform = myScaleTransform;

                rcChild.X += previousChildSize;
                previousChildSize = child.DesiredSize.Width * scalePct;
                rcChild.Width = child.DesiredSize.Width;
                rcChild.Height = Math.Max(arrangeSize.Height, child.DesiredSize.Height);

                child.Arrange(rcChild);
            }
        }

        private void ResizeVertically(Size arrangeSize)
        {
            if (Height == 0)
            {
                return;
            }

            // Get the total height of all the children combined
            var childrenHeight = (from UIElement child in InternalChildren where child != null select child.DesiredSize.Height).Sum();

            // Adjust the max allowed height to account for any padding
            var maxAllowedHeight = Height - Padding.Top - Padding.Bottom;

            // If the childrenHeight is greater than the maxAllowedHeight, scale down
            var scalePct = maxAllowedHeight > 0 && childrenHeight > maxAllowedHeight
                ? maxAllowedHeight / childrenHeight
                : 1.0;

            // Shift the starting position of the content according to the alignment choice
            var amountPerSideToCenter = Math.Max(0, Height - childrenHeight) / 2.0;
            var rcChild = new Rect(arrangeSize);
            var previousChildSize = 0.0d;
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    previousChildSize = Math.Max(0, Padding.Top);
                    break;
                case VerticalAlignment.Stretch:
                case VerticalAlignment.Center:
                    previousChildSize = Math.Max(Padding.Top, amountPerSideToCenter);
                    break;
                case VerticalAlignment.Bottom:
                    previousChildSize = Math.Min(Height - childrenHeight, Height - (childrenHeight + Padding.Bottom));
                    break;
            }

            // Apply the scaling and position the items
            foreach (UIElement child in InternalChildren)
            {
                if (child == null)
                {
                    continue;
                }

                var myScaleTransform = new ScaleTransform { ScaleY = scalePct, ScaleX = 1.0 };
                child.RenderTransform = myScaleTransform;

                rcChild.Y += previousChildSize;
                previousChildSize = child.DesiredSize.Height * scalePct;
                rcChild.Height = child.DesiredSize.Height;
                rcChild.Width = Math.Max(arrangeSize.Width, child.DesiredSize.Width);

                child.Arrange(rcChild);
            }
        }
    }
}
