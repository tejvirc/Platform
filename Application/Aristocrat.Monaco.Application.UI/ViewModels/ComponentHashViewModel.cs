namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class ComponentHashViewModel : BaseEntityViewModel
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