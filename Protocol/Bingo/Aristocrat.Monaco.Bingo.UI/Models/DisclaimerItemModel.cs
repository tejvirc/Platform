namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;

    public class DisclaimerItemModel : ObservableObject
    {
        private string _text;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }
}