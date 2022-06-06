namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using MVVM.Model;

    public class DisclaimerItemModel : BaseNotify
    {
        private string _text;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }
}