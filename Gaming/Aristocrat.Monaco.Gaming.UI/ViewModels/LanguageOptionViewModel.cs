namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Globalization;

    using MVVM.ViewModel;

    public class LanguageOptionViewModel : BaseViewModel
    {
        private bool _isDefault;
        private bool _isEnabled;
        private bool _canEditIsDefault;
        private bool _canEditIsEnabled;

        public LanguageOptionViewModel(CultureInfo cultureInfo, bool isEnabled, bool isMandatoryLanguage)
        {
            CultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
            Language = GetDisplayName(cultureInfo);
            _isEnabled = isEnabled;
            IsMandatoryLanguage = isMandatoryLanguage;
            UpdateCanEditIsEnabled();
            CanEditIsDefault = IsEnabled;
        }

        public bool IsMandatoryLanguage { get; }

        public string Language { get; }

        public CultureInfo CultureInfo { get; }

        public bool CanEditIsEnabled
        {
            get => _canEditIsEnabled;
            private set
            {
                if (_canEditIsEnabled == value)
                {
                    return;
                }

                _canEditIsEnabled = value;
                RaisePropertyChanged(nameof(CanEditIsEnabled));
            }
        }

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
                CanEditIsDefault = _isEnabled;
                if (!_isEnabled)
                {
                    this.IsDefault = false;
                }

                RaisePropertyChanged(nameof(IsEnabled));
            }
        }

        public bool CanEditIsDefault
        {
            get => _canEditIsDefault;
            private set
            {
                if (_canEditIsDefault == value)
                {
                    return;
                }

                _canEditIsDefault = value;
                RaisePropertyChanged(nameof(CanEditIsDefault));
            }
        }

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
                UpdateCanEditIsEnabled();
                RaisePropertyChanged(nameof(IsDefault));
            }
        }

        private static string GetDisplayName(CultureInfo cultureInfo)
        {
            var displayName = cultureInfo.DisplayName;
            var index = displayName.IndexOf('(');
            return index == -1 ? displayName : displayName.Substring(0, index).Trim();
        }

        private void UpdateCanEditIsEnabled() => CanEditIsEnabled = !IsMandatoryLanguage;
    }
}
