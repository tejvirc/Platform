namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Contracts.GameSpecificOptions;

    /// <summary>
    ///     Data model for GameSpecificOption
    /// </summary>
    public class GameSpecificOptionModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private string _value;
        private Dictionary<string, IEnumerable<string>> _errors = new Dictionary<string, IEnumerable<string>>();

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void OnErrorChanged([CallerMemberName] string propertyName = null) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        /// <summary>
        ///    Event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///    Event handler for INotifyDataErrorInfo
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        ///    Implement INotifyDataErrorInfo HasErrors
        /// </summary>
        public bool HasErrors => _errors.Values.Any(err => err != null);

        /// <summary>
        ///    Implement INotifyDataErrorInfo GetErrors
        /// </summary>
        public IEnumerable GetErrors(string propertyName) => _errors.ContainsKey(propertyName) ? _errors[propertyName] : Enumerable.Empty<object>();

        /// <summary>
        ///    Game Specific Option name defined by game studio
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///    Game Specific Option value, original value set by game studio
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;

                    if (OptionType == OptionType.Number)
                    {
                        ValidateNumber();
                        // Don't need to handle other cases
                        OnPropertyChanged();
                    }
                }

            }
        }

        /// <summary>
        ///      To define the option type
        /// </summary>
        public OptionType OptionType { get; set; }

        /// <summary>
        ///      For List Type, value set defined by game studio
        ///      For Toggle Type, predefined {"On", "Off"}
        ///      Not used by Number type
        /// </summary>
        public List<string> ValueSet { get; set; }

        /// <summary>
        ///     Minimum value allowed, ToggleType Number only
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        ///      Maximum value allowed, ToggleType Number only
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        ///      Flag to check if it is a DropDown
        /// </summary>
        public bool IsDropDownField { get; set; }

        /// <summary>
        ///      Flag to check if it is a TextBox
        /// </summary>
        public bool IsTextBoxField { get; set; }

        /// <summary>
        ///      Validate TextBox input against MinValue and Maximum
        /// </summary>
        private void ValidateNumber()
        {
            var propertyName = nameof(Value);
            int val;

            if (string.IsNullOrEmpty(Value) || !int.TryParse(Value, out val))
            {
                _errors[propertyName] = new[] { "Input must be a number." };
            }
            else if (val < MinValue)
            {
                _errors[propertyName] = new[] { "Number cannot be less than minValue=" + MinValue + "." };
            }
            else if (val > MaxValue)
            {
                _errors[propertyName] = new[] { "Number cannot be larger than maxValue=" + MaxValue + "." };
            }
            else
            {
                if (_errors.ContainsKey(propertyName))
                {
                    _errors.Remove(propertyName);
                }
            }

            OnErrorChanged(propertyName);
        }
    }
}