namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;

    [CLSCompliant(false)]
    public class SoundFileViewModel : ObservableObject
    {
        private string _name;
        private string _path;

        public SoundFileViewModel(string name, string path)
        {
            _name = name;
            _path = path;
        }

        public string Name
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

        public string Path
        {
            get => _path;

            set
            {
                if (_path == value)
                {
                    return;
                }

                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }
    }
}