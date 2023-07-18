namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;

    [CLSCompliant(false)]
    public class SoundFileViewModel : BaseViewModel
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
                RaisePropertyChanged(nameof(Name));
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
                RaisePropertyChanged(nameof(Path));
            }
        }
    }
}
