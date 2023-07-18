namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;

    [CLSCompliant(false)]
    public class ComponentHashViewModel : ObservableObject
    {
        public string ComponentId { get; set; }

        public string HashResult { get; set; }

        public void ChangeHashResult(string value)
        {
            HashResult = value;
            RaisePropertyChanged(nameof(HashResult));
        }
    }
}
