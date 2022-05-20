namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using MVVM.Model;

    public class ResponsibleGamingInfoPage : BaseNotify
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
