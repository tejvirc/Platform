namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using ViewModels;
    using Xceed.Wpf.Toolkit;

    /// <summary>
    ///     Interaction logic for MechanicalReelsPage.xaml
    /// </summary>
    public sealed partial class MechanicalReelsPage
    {
        public MechanicalReelsPage()
        {
            InitializeComponent();
        }

        private void IntegerUpDown_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is not IntegerUpDown upDown)
            {
                return;
            }

            var newValue = (int)e.NewValue;
            var oldValue = (int)e.OldValue;

            if (!((MechanicalReelsPageViewModel)DataContext).IsAnimationController || Math.Abs(newValue) != 2)
            {
                return;
            }

            if (oldValue < newValue)
            {
                upDown.Value++;
            }
            
            if (oldValue > newValue)
            {
                upDown.Value--;
            }
        }
    }
}
