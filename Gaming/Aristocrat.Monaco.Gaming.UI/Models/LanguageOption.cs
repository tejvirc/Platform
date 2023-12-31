﻿namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using MVVM.ViewModel;

    public class LanguageOption : BaseViewModel
    {
        private bool _isDefault;
        private bool _isEnabled;

        public LanguageOption(string language, bool isEnabled, bool canMakeDefault, bool isDefault)
        {
            Language = language;
            _isEnabled = isEnabled;
            CanMakeDefault = canMakeDefault;
            _isDefault = isDefault;
        }

        public string Language { get; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                {
                    return;
                }

                _isEnabled = value;

                if (!_isEnabled)
                {
                    IsDefault = false;
                    // TODO Does there need to be a default at all times?
                }

                RaisePropertyChanged(nameof(IsEnabled));
            }
        }

        public bool CanMakeDefault { get; }

        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                if (_isDefault == value)
                {
                    return;
                }

                _isDefault = value;
                RaisePropertyChanged(nameof(IsDefault));
            }
        }
    }
}
