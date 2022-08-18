namespace Aristocrat.Monaco.Gaming.UI.Views.Controls
{
    using System;
    using System.Windows;
    using Panel = System.Windows.Controls.Panel;

    /// <summary>
    ///     Custom layout panel to layout game icons dynamically based on the number of
    ///     games on the page.  See http://jerry.ali.local/browse/OZGAMEDBD-85 for the
    ///     desired layouts for 8, 9, 10, 11, and 12 game counts.
    ///     https://msdn.microsoft.com/en-us/library/ms745058(v=vs.110).aspx#LayoutSystem_Measure_Arrange
    /// </summary>
    public class GameLayoutPanel : Panel
    {
        /// <summary>  SpacingProperty </summary>
        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(
                nameof(Spacing),
                typeof(Size),
                typeof(GameLayoutPanel),
                new FrameworkPropertyMetadata(new Size(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsTabViewProperty =
            DependencyProperty.Register(
                nameof(IsTabView),
                typeof(bool),
                typeof(GameLayoutPanel),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsExtraLargeGameIconTabActiveProperty =
            DependencyProperty.Register(
                nameof(IsExtraLargeGameIconTabActive),
                typeof(bool),
                typeof(GameLayoutPanel),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        private double _itemHeight;
        private double _itemWidth;
        private Size _measuredSize;
        private Size _surfaceSize;

        /// <summary>
        ///     Gets or sets a value indicating whether the window is activated or not.
        /// </summary>
        public Size Spacing
        {
            get => (Size)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public bool IsTabView
        {
            get => (bool)GetValue(IsTabViewProperty);
            set => SetValue(IsTabViewProperty, value);
        }

        /// <summary>
        ///     Is the current tab hosting extra large game icons
        /// </summary>
        public bool IsExtraLargeGameIconTabActive
        {
            get => (bool)GetValue(IsExtraLargeGameIconTabActiveProperty);
            set => SetValue(IsExtraLargeGameIconTabActiveProperty, value);
        }

        /// <summary>
        ///     https://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.measureoverride(v=vs.110).aspx
        ///     Override MeasureOverride to implement custom layout sizing behavior for your element as it participates
        ///     in the Windows Presentation Foundation (WPF) layout system. Your implementation should do the following:
        ///     1. Iterate your element's particular collection of children that are part of layout, call Measure
        ///     on each child element.
        ///     2. Immediately get DesiredSize on the child(this is set as a property after Measure is called).
        ///     3. Compute the net desired size of the parent based upon the measurement of the child elements.
        /// </summary>
        /// <param name="availableSize">The size available.</param>
        /// <returns>The return value of MeasureOverride should be the element's own desired size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            var childCount = Children.Count;
            if (childCount == 0)
            {
                return base.MeasureOverride(availableSize);
            }

            var count = IsExtraLargeGameIconTabActive
                ? GameRowColumnCalculator.ExtraLargeIconRowColCount
                : GameRowColumnCalculator.CalculateRowColCount(childCount);

            var spacingX = Spacing.Width * (count.Cols - 1);
            var spacingY = Spacing.Height * (count.Rows - 1);

            const double tabHeightScale = 0.86;
            var availableHeight = IsTabView ? availableSize.Height * tabHeightScale : availableSize.Height;

            var childWidth = (availableSize.Width - spacingX) / count.Cols;
            var childHeight = (availableHeight - spacingY) / count.Rows;

            if (childWidth < 0 || childHeight < 0)
            {
                return base.MeasureOverride(availableSize);
            }

            var maxSize = new Size(0.0, 0.0);
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(childWidth, childHeight));

                maxSize.Width = Math.Max(child.DesiredSize.Width, maxSize.Width);
                maxSize.Height = Math.Max(child.DesiredSize.Height, maxSize.Height);
            }

            // All items should have the same size.
            //_itemWidth = maxSize.Width;
            //_itemHeight = maxSize.Height;

            // don't think we should take the max width/height, should just use the calculated width/height
            _itemWidth = childWidth;
            _itemHeight = childHeight;

            _measuredSize = new Size(count.Cols * _itemWidth + spacingX, count.Rows * _itemHeight + spacingY);

            _surfaceSize = availableSize;

            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        ///     Positions child elements and determines a size for a FrameworkElement derived class.
        /// </summary>
        /// <param name="finalSize">
        ///     The final area within the parent that this element should
        ///     use to arrange itself and its children.
        /// </param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var childCount = Children.Count;

            var count = IsExtraLargeGameIconTabActive
                ? GameRowColumnCalculator.ExtraLargeIconRowColCount
                : GameRowColumnCalculator.CalculateRowColCount(childCount);

            var gamesInLastRow = childCount > 0 ? childCount % count.Cols : 0;
            if  (gamesInLastRow == 0 && childCount > 0)
            {
                gamesInLastRow = count.Cols;
            }

            var columnDefinition = new int[count.Rows];

            for (int i = 0; i < count.Rows - 1; i++)
            {
                columnDefinition[i] = count.Cols;
            }

            columnDefinition[count.Rows - 1] = gamesInLastRow;

            ArrangeGameLayout(count.Rows, count.Cols, columnDefinition);

            return finalSize;
        }

        private void ArrangeGameLayout(int rowCount, int columnsInFullRow, int[] columnCounts)
        {
            // Center
            var offsetX = 0.5 * _surfaceSize.Width - 0.5 * _measuredSize.Width;

            var dx = Spacing.Width;
            var dy = Spacing.Height;

            var k = 0;
            for (var i = 0; i < rowCount; ++i)
            {
                var colCount = columnCounts[i];

                var y = i * _itemHeight;

                for (var j = 0; j < colCount; ++j)
                {
                    var child = Children[k + j];

                    var x = j * _itemWidth;

                    if (Children.Count % columnsInFullRow != 0 && i == rowCount - 1)
                    {
                        //get differential
                        var columnDifferential = columnsInFullRow - colCount;

                        x += (columnDifferential * .5) * (_itemWidth + Spacing.Width);
                    }

                    var globalX = offsetX + (x + j * dx) - 15;
                    var globalY = y + i * dy;

                    // TODO: Kind of hacky, but the icons with jackpot in 10+ game layout
                    // need extra area to fit "new-star" icon that goes out of the rectangle bounds.
                    const double extraArea = 0; // 20.0;
                    child.Arrange(new Rect(globalX, globalY, _itemWidth, _itemHeight + extraArea));
                }

                k += colCount;
            }
        }
    }
}

