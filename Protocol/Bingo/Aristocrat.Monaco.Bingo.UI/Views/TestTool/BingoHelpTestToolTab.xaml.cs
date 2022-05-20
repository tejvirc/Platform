namespace Aristocrat.Monaco.Bingo.UI.Views.TestTool
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using ViewModels.TestTool;

    /// <summary>
    /// Interaction logic for BingoHelpTestToolTab.xaml
    /// </summary>
    public partial class BingoHelpTestToolTab
    {
        public BingoHelpTestToolTab()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Parent is TabControl parentTabControl)
            {
                parentTabControl.SelectionChanged += ParentTabControlSelectionChanged;
            }
        }

        private void ParentTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Contains(this))
            {
                ViewModel.AnnounceVisibility(true);
            }
            else if (e.RemovedItems.Contains(this))
            {
                ViewModel.AnnounceVisibility(false);
            }
        }

        /// <summary>
        ///     Gets or sets the view model for the view.
        /// </summary>
        public BingoHelpTestToolViewModel ViewModel
        {
            get => DataContext as BingoHelpTestToolViewModel;
            set => DataContext = value;
        }

        private void TopGridSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var topHeight = HelpBoxGrid.RowDefinitions[0].ActualHeight;
            var fullHeight = topHeight + HelpBoxGrid.RowDefinitions[2].ActualHeight +
                             HelpBoxGrid.RowDefinitions[4].ActualHeight;
            ViewModel.HelpBoxTop = Math.Round(topHeight / fullHeight, 2);
        }

        private void LeftGridSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var leftWidth = HelpBoxGrid.ColumnDefinitions[0].ActualWidth;
            var fullWidth = leftWidth + HelpBoxGrid.ColumnDefinitions[2].ActualWidth +
                            HelpBoxGrid.ColumnDefinitions[4].ActualWidth;
            ViewModel.HelpBoxLeft = Math.Round(leftWidth / fullWidth, 2);
        }

        private void RightGridSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var rightWidth = HelpBoxGrid.ColumnDefinitions[4].ActualWidth;
            var fullWidth = rightWidth + HelpBoxGrid.ColumnDefinitions[2].ActualWidth +
                            HelpBoxGrid.ColumnDefinitions[0].ActualWidth;
            ViewModel.HelpBoxRight = Math.Round(rightWidth / fullWidth, 2);
        }

        private void BottomGridSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var bottomHeight = HelpBoxGrid.RowDefinitions[4].ActualHeight;
            var fullHeight = bottomHeight + HelpBoxGrid.RowDefinitions[2].ActualHeight +
                             HelpBoxGrid.RowDefinitions[0].ActualHeight;
            ViewModel.HelpBoxBottom = Math.Round(bottomHeight / fullHeight, 2);
        }
    }
}
