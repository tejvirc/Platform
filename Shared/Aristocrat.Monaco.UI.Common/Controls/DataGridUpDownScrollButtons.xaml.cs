namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     This class provides buttons for scrolling a Listbox with only move up and down
    /// </summary>
    public partial class DataGridUpDownScrollButtons : INotifyPropertyChanged
    {
        private bool _initialized;
        private readonly object _lock = new object();
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Aristocrat.Monaco.UI.Common.Controls.DataGridUpDownScrollButtons" /> class.
        /// </summary>
        public DataGridUpDownScrollButtons()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     The ScrollViewer for the Listbox to be scrolled
        /// </summary>
        public ScrollViewer ScrollGrid
        {
            get => (ScrollViewer)GetValue(ScrollGridProperty);
            set => SetValue(ScrollGridProperty, value);
        }

        /// <summary>
        ///     DependencyProperty for binding DataGrid ScrollViewer
        /// </summary>
        public static readonly DependencyProperty ScrollGridProperty = DependencyProperty.Register(
            "ScrollGrid",
            typeof(ScrollViewer),
            typeof(DataGridUpDownScrollButtons),
            new PropertyMetadata(default(ScrollViewer), ScrollGrid_Changed));

        /// <summary>
        ///     Whether or not to show the scroll buttons
        ///     Scroll buttons should be visible if either:
        ///     1. ScrollViewer.CanContentScroll is set to true on the Listbox AND there is at least one item to scroll
        ///     2. There are enough rows to require scrolling (ActualHeight > ScrollableHeight)
        /// </summary>
        public bool CanContentScroll => ScrollGrid != null &&
                                        ScrollGrid.ScrollableHeight > 0 &&
                                        (ScrollGrid.CanContentScroll ||
                                         ScrollGrid.ActualHeight > ScrollGrid.ScrollableHeight);

        private static void ScrollGrid_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if  (d is DataGridUpDownScrollButtons dataGridScrollButtons)
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

        private void PreviousButtonClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.LineUp();
        }
        
        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.LineDown();
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
