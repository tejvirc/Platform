namespace Aristocrat.Monaco.Application.UI.ViewModels.NoteAcceptor
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.Extensions;

    [CLSCompliant(false)]
    public class ConfigurableDenomination : ObservableObject
    {
        private bool _enabled;
        private bool _selected;
        private bool _visible;

        public ConfigurableDenomination(int denom, RelayCommand<bool> command, bool selected)
        {
            Denom = denom;
            ChangeCommand = command;
            _selected = selected;
            _enabled = true;
            _visible = true;
        }

        public int Denom { get; }

        public RelayCommand<bool> ChangeCommand { get; }

        public string DisplayValue => Denom.FormattedCurrencyString("C0");

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
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
                    OnPropertyChanged(nameof(Selected));
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
                    OnPropertyChanged(nameof(Visible));
                }
            }
        }
    }
}