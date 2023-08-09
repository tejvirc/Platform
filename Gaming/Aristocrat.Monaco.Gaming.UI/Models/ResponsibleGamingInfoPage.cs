namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;

    public class ResponsibleGamingInfoPage : ObservableObject
    {
        private int _index;
        private string _backgroundKey;

        public int Index
        {
            get => _index;

            set => SetProperty(ref _index, value);
        }

        public string BackgroundKey
        {
            get => _backgroundKey;

            set => SetProperty(ref _backgroundKey, value);
        }
    }
}
