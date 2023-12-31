﻿namespace Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using ViewModels.OperatorMenu;

    /// <summary>
    /// Interaction logic for ProgressiveMetersPage.xaml
    /// </summary>
    public partial class ProgressiveMetersPage
    {
        public ProgressiveMetersPage()
        {
            InitializeComponent();
            DataContextChanged += ProgressiveMetersPage_DataContextChanged;
        }

        private void ProgressiveMetersPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ProgressiveMetersPageViewModel) DataContext;
            foreach (var meterNode in viewModel.MeterNodes)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn
                {
                    Header = meterNode.DisplayName, Binding = new Binding($"[{meterNode.DisplayName}].Value"),
                    Width = DataGridLength.Auto
                };
                ProgressiveDataGrid?.Columns.Add(textColumn);
            }
        }

    }
}
