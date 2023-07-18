namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    public class ResponsibleGamingInfoPage : BaseObservableObject
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
