namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    public partial class ManualHandicapHorseNumber
    {
        // Button click dependency property.
        public static readonly DependencyProperty OnClickHandlerProperty =
            DependencyProperty.Register(
                nameof(OnClickHandler),
                typeof(ICommand),
                typeof(ManualHandicapHorseNumber));

        // Horse number dependency property.
        public static readonly DependencyProperty HorseNumberProperty =
            DependencyProperty.Register(
                nameof(HorseNumber),
                typeof(int),
                typeof(ManualHandicapHorseNumber),
                new PropertyMetadata(3));

        // Horse selected dependency property.
        public static readonly DependencyProperty HorseSelectedProperty =
            DependencyProperty.Register(
                nameof(HorseSelected),
                typeof(bool),
                typeof(ManualHandicapHorseNumber),
                new PropertyMetadata(false));

        public ManualHandicapHorseNumber()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Button click action
        /// </summary>
        public ICommand OnClickHandler
        {
            get => (ICommand)GetValue(OnClickHandlerProperty);
            set => SetValue(OnClickHandlerProperty, value);
        }

        /// <summary>
        /// Horse number value
        /// </summary>
        public int HorseNumber
        {
            get => (int)GetValue(HorseNumberProperty);
            set => SetValue(HorseNumberProperty, value);
        }

        /// <summary>
        /// Horse is selected
        /// </summary>
        public bool HorseSelected
        {
            get => (bool)GetValue(HorseSelectedProperty);
            set => SetValue(HorseSelectedProperty, value);
        }

        // Button click event.
        public event EventHandler OnClickEvent;

        // Button click event.
        private void HorseNumber_Click(object sender, RoutedEventArgs e)
        {
            OnClickHandler?.Execute(this);
            OnClickEvent?.Invoke(this, e);
        }

    }
}