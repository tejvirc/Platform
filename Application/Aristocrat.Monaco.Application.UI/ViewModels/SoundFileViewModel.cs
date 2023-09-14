namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Hardware.Contracts.Audio;

    [CLSCompliant(false)]
    public class SoundFileViewModel : ObservableObject
    {
        private SoundName _name;
        private string _description;

        public SoundFileViewModel(SoundName name, string description)
        {
            _name = name;
            _description = description;
        }

        public SoundName Name
        {
            get => _name;

            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Description
        {
            get => _description;

            set
            {
                if (_description == value)
                {
                    return;
                }

                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }
}
