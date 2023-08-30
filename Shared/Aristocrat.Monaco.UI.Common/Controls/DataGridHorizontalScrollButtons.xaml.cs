namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for DataGridHorizontalScrollButtons.xaml
    /// </summary>
    public partial class DataGridHorizontalScrollButtons : INotifyPropertyChanged
    {
        /// <summary>
        ///     DependencyProperty for binding DataGrid ScrollViewer
        /// </summary>
        public static readonly DependencyProperty ScrollGridProperty = DependencyProperty.Register(
            "ScrollGrid",
            typeof(ScrollViewer),
            typeof(DataGridHorizontalScrollButtons),
            new PropertyMetadata(default(ScrollViewer), ScrollGrid_Changed));

        private readonly object _lock = new object();
        private bool _initialized;

        /// <summary>
        ///     Ctor
        /// </summary>
        public DataGridHorizontalScrollButtons()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     The ScrollViewer for the DataGrid to be scrolled
        /// </summary>
        public ScrollViewer ScrollGrid
        {
            get => (ScrollViewer)GetValue(ScrollGridProperty);
            set => SetValue(ScrollGridProperty, value);
        }

        /// <summary>
        ///     Whether or not to show the scroll buttons
        ///     Scroll buttons should be visible if either:
        ///     1. ScrollViewer.CanContentScrollHorizontal is set to true on the DataGrid AND there is at least one item to scroll
        ///     2. There are enough columns to require scrolling (ActualWidth > ScrollableLength)
        /// </summary>
        public bool CanContentScroll => ScrollGrid != null &&
                                        (ScrollGrid.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible || ScrollGrid.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto) &&
                                        ScrollGrid.ScrollableWidth > 0 &&
                                        (ScrollGrid.CanContentScroll || ScrollGrid.ActualWidth > ScrollGrid.ScrollableWidth);

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        private static void ScrollGrid_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridHorizontalScrollButtons dataGridScrollButtons)
            {
                dataGridScrollButtons.SetupScrollGrid();
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateProperties();
        }

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(CanContentScroll));
        }

        private void PageLeftMostButton_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.ScrollToLeftEnd();
        }

        private void PageLeftButton_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.PageLeft();
        }

        private void PreviousButtonClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.LineLeft();
        }

        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.LineRight();
        }

        private void PageRightButton_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.PageRight();
        }

        private void PageRightMostButton_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.ScrollToRightEnd();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupScrollGrid();
            UpdateProperties();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            lock (_lock)
            {
                if (ScrollGrid != null)
                {
                    ScrollGrid.ScrollChanged -= OnScrollChanged;
                    ScrollGrid.SizeChanged -= OnScrollGridSizeChanged;
                }

                _initialized = false;
            }
        }

        private void OnScrollGridSizeChanged(object sender, SizeChangedEventArgs args)
        {
            UpdateProperties();
        }

        private void SetupScrollGrid()
        {
            lock (_lock)
            {
                if (ScrollGrid != null && !_initialized)
                {
                    ScrollGrid.ScrollChanged += OnScrollChanged;
                    ScrollGrid.SizeChanged += OnScrollGridSizeChanged;
                    _initialized = true;
                }
            }
        }
    }
}