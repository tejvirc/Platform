namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    public class DisclaimerItemModel : BaseObservableObject
    {
        private string _text;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }
}