namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// This class provides buttons for scrolling a DataGrid
    /// </summary>
    public partial class DataGridScrollButtons : INotifyPropertyChanged
    {
        private bool _initialized;
        private bool _hasListBoxParent;
        private readonly object _lock = new object();
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Aristocrat.Monaco.UI.Common.Controls.DataGridScrollButtons" /> class.
        /// </summary>
        public DataGridScrollButtons()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     The ScrollViewer for the DataGrid to be scrolled
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
            typeof(DataGridScrollButtons),
            new PropertyMetadata(default(ScrollViewer), ScrollGrid_Changed));

        /// <summary>
        /// Whether or not to show the scroll buttons
        /// Scroll buttons should be visible if either:
        /// 1. ScrollViewer.CanContentScroll is set to true on the DataGrid AND there is at least one item to scroll
        /// 2. There are enough rows to require scrolling (ActualHeight > ScrollableHeight)
        /// </summary>
        public bool CanContentScroll => ScrollGrid != null &&
                                        ScrollGrid.ScrollableHeight > 0 &&
                                        (ScrollGrid.CanContentScroll ||
                                         ScrollGrid.ActualHeight > ScrollGrid.ScrollableHeight);

        private static void ScrollGrid_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if  (d is DataGridScrollButtons dataGridScrollButtons)
            {
                dataGridScrollButtons.SetupScrollGrid();
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateProperties();
        }

        private void RaisePropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateProperties()
        {
            if ((ScrollGrid!=null && ScrollGrid.CanContentScroll)|| !_hasListBoxParent)
            {
                RaisePropertyChanged(nameof(CanContentScroll));
            }
        }

        private void PageTopButton_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollGrid?.ScrollToHome();
        }
        
        private void PageUpButton_OnClick(object sender, RoutedEventArgs e)
        {
            EnableInternalScrollForListBoxScroll((FrameworkElement)sender, false);
            ScrollGrid?.PageUp();
        }
        
        private void PreviousButtonClick(object sender, RoutedEventArgs e)
        {
            DisableInternalScrollForListBoxScroll((FrameworkElement)sender);
            ScrollGrid?.LineUp();
        }
        
        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            DisableInternalScrollForListBoxScroll((FrameworkElement)sender);
            ScrollGrid?.LineDown();

        }
        
        private void PageDownButton_OnClick(object sender, RoutedEventArgs e)
        {
            EnableInternalScrollForListBoxScroll((FrameworkElement)sender, true);
            ScrollGrid?.PageDown();
        }
        
        private void PageBottomButton_OnClick(object sender, RoutedEventArgs e)
        {
            DisableInternalScrollForListBoxScroll((FrameworkElement)sender);
            ScrollGrid?.ScrollToBottom();
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

        // Based on the remark in MSDN about ScrollViewer.LineUp and LineDown:
        // If you require physical scrolling instead of logical scrolling, wrap the host Panel element in a ScrollViewer and
        // set its CanContentScroll property to false.
        // This method is temporarily set CanContentScroll to false for ListView line by line scrolling
        private void DisableInternalScrollForListBoxScroll(FrameworkElement element)
        {
            if (element is null) return;

            // normally TemplatedParent is null for most elements, for the scrollbar buttons we know they are the descendent of either
            // ListBox or DataGrid in the visual tree, so when the TemplatedParent is not null, we know we found the true ancestor
            while (element.TemplatedParent is null)
            {
                element = (FrameworkElement)element.Parent;
            }

            if (element.TemplatedParent is ListBox listbox && ScrollGrid.CanContentScroll)
            {
                double trueOffset = 0;
                for (var i = 0; i < (int)ScrollGrid.VerticalOffset; i++)
                {
                    if (listbox.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container)
                    {
                        trueOffset += container.ActualHeight;
                    }
                }

                ScrollGrid.CanContentScroll = false;
                ScrollGrid.ScrollToVerticalOffset(trueOffset);
                _hasListBoxParent = true;
            }
            else if (element.TemplatedParent is DataGrid)
            {
                ScrollGrid.CanContentScroll = true;
                _hasListBoxParent = false;
            }
        }

        private void EnableInternalScrollForListBoxScroll(FrameworkElement element, bool scrollingDown)
        {
            if (ScrollGrid.CanContentScroll || element is null) return;

            while (element.TemplatedParent is null)
            {
                element = (FrameworkElement)element.Parent;
            }

            if (element.TemplatedParent is ListBox listbox)
            {

                if (scrollingDown && !CanScrollFurther(listbox))
                {
                    ScrollGrid.ScrollToBottom();
                    ScrollGrid.CanContentScroll = false;
                    _hasListBoxParent = true;
                    return;
                }

                var trueOffset = ScrollGrid.VerticalOffset;
                ScrollGrid.CanContentScroll = true;
                int index = 0;
                while (trueOffset > 0)
                {
                    var item = (FrameworkElement)listbox.ItemContainerGenerator.ContainerFromIndex(index);
                    if (item is null) break;

                    trueOffset -= item.ActualHeight;
                    if (trueOffset > 0)
                        index++;
                }

                ScrollGrid.ScrollToVerticalOffset(index);
                _hasListBoxParent = true;
                return;
            }
            ScrollGrid.CanContentScroll = true;
        }

        private bool CanScrollFurther(ListBox listbox)
        {
            // Do not scroll if we are passed the start of the last element
            var lastElementStartinglocation = GetStartingLocationOfLastElement(listbox);
            if (ScrollGrid.VerticalOffset >= lastElementStartinglocation)
            {
                return false;
            }

            return true;
        }

        private double GetStartingLocationOfLastElement(ListBox listBox)
        {
            double size = 0;
            for (var i = 0; i < listBox.ItemContainerGenerator.Items.Count - 1; i++)
            {
                if (listBox.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container)
                {
                    size += container.ActualHeight;
                }
            }

            return size;
        }
    }
}
