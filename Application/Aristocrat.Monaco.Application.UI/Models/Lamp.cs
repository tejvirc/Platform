namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    [CLSCompliant(false)]
    public class Lamp : BaseObservableObject
    {
        private uint _bit;
        private bool _state;
        private string _title;

        public string Title
        {
            get => _title;

            set
            {
                if (_title == value)
                {
                    return;
                }

                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public uint Bit
        {
            get => _bit;

            set
            {
                if (_bit == value)
                {
                    return;
                }

                _bit = value;
                OnPropertyChanged(nameof(Bit));
            }
        }

        public bool State
        {
            get => _state;

            set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
    }
}