namespace Aristocrat.Monaco.Application.UI.ViewModels.NoteAcceptor
{
    using System;
    using System.Globalization;
    using Contracts.Extensions;

    [CLSCompliant(false)]
    public class ConfigurableDenomination : BaseViewModel
    {
        private bool _enabled;
        private bool _selected;
        private bool _visible;
        private CultureInfo _currentCultureInfo = CurrencyExtensions.CurrencyCultureInfo;

        public ConfigurableDenomination(int denom, ActionCommand<bool> command, bool selected)
        {
            Denom = denom;
            ChangeCommand = command;
            _selected = selected;
            _enabled = true;
            _visible = true;
        }

        public int Denom { get; }

        public ActionCommand<bool> ChangeCommand { get; }

        public string DisplayValue => Denom.ToString("C0", _currentCultureInfo);

        public void UpdateProps(CultureInfo cultureInfo = null)
        {
            _currentCultureInfo = cultureInfo ?? CurrencyExtensions.CurrencyCultureInfo;

            RaisePropertyChanged(nameof(DisplayValue));
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    RaisePropertyChanged(nameof(Enabled));
                }
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    RaisePropertyChanged(nameof(Selected));
                    ChangeCommand.Execute(Selected);
                }
            }
        }

        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    RaisePropertyChanged(nameof(Visible));
                }
            }
        }
    }
}
