namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Xceed.Wpf.Toolkit;

    /// <summary>
    ///     A Currency Decimal Up Down control that provides a list of values to cycle through.
    /// </summary>
    public class SelectableCurrencyDecimalUpDown : CurrencyDecimalUpDown
    {
        private int _selectedIndex = -1;

        static SelectableCurrencyDecimalUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SelectableCurrencyDecimalUpDown),
                new FrameworkPropertyMetadata(typeof(SelectableCurrencyDecimalUpDown)));
        }

        /// <summary>
        ///     Dependency Property for the selectable values
        /// </summary>
        public static readonly DependencyProperty SelectableValuesProperty = DependencyProperty.Register(
            nameof(SelectableValues),
            typeof(IList<decimal>),
            typeof(SelectableCurrencyDecimalUpDown),
            new FrameworkPropertyMetadata(new List<decimal>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     The selectable values to cycle through using the up down control
        /// </summary>
        public IList<decimal> SelectableValues
        {
            get => (IList<decimal>)GetValue(SelectableValuesProperty);
            set
            {
                SetValue(SelectableValuesProperty, value);
                OnValueChanged(Value, Value); // Reset the location information
                SetValidSpinDirection(); // Reset the increment
            }
        }

        /// <inheritdoc />
        protected override decimal IncrementValue(decimal value, decimal increment)
        {
            if (SelectableValues == null)
            {
                return value;
            }

            return _selectedIndex >= (SelectableValues.Count - 1)
                ? SelectableValues.LastOrDefault()
                : SelectableValues[++_selectedIndex];
        }

        /// <inheritdoc />
        protected override decimal DecrementValue(decimal value, decimal increment)
        {
            if (SelectableValues == null)
            {
                return value;
            }

            return _selectedIndex <= 0 ? SelectableValues.FirstOrDefault() : SelectableValues[--_selectedIndex];
        }

        /// <inheritdoc />
        protected override void SetValidSpinDirection()
        {
            if (Value.HasValue && Spinner != null)
            {
                var directions = ValidSpinDirections.None;
                if (_selectedIndex > 0 && SelectableValues.Any())
                {
                    directions |= ValidSpinDirections.Decrease;
                }

                if ((SelectableValues?.Any() ?? false) && SelectableValues.Count - 1 > _selectedIndex)
                {
                    directions |= ValidSpinDirections.Increase;
                }

                Spinner.ValidSpinDirection = directions;
            }
            else
            {
                base.SetValidSpinDirection();
            }
        }

        /// <inheritdoc />
        protected override void OnValueChanged(decimal? oldValue, decimal? newValue)
        {
            if (newValue.HasValue && SelectableValues != null)
            {
                _selectedIndex = SelectableValues.IndexOf(newValue.Value);
            }
            else
            {
                _selectedIndex = -1;
            }

            base.OnValueChanged(oldValue, newValue);
        }
    }
}