namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;
    using System;

    [CLSCompliant(false)]
    public class ComponentHashViewModel : BaseObservableObject
    {
        public string ComponentId { get; set; }

        public string HashResult { get; set; }

        public void ChangeHashResult(string value)
        {
            HashResult = value;
            OnPropertyChanged(nameof(HashResult));
        }
    }
}
