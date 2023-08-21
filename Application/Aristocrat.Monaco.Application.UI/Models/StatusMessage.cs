namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Contracts.Localization;
    using Kernel;
    using MVVM.Model;

    [CLSCompliant(false)]
    public class StatusMessage : BaseNotify
    {
        private readonly DisplayableMessage _displayableMessage;
        private readonly string _resourceKey;

        public StatusMessage(DisplayableMessage displayableMessage, string resourceKey)
        {
            _displayableMessage = displayableMessage;
            AdditionalInfoCallback = displayableMessage.HelpTextCallback;
            _resourceKey = resourceKey;
        }

        public string Message => _displayableMessage.Message;

        public Guid? MessageId => _displayableMessage?.Id;

        public string AdditionalInfo
        {
            get
            {
                if (AdditionalInfoCallback != null)
                {
                    return AdditionalInfoCallback.Invoke();
                }

                if (!string.IsNullOrWhiteSpace(_resourceKey))
                {
                    return Localizer.For(CultureFor.Operator).GetString(_resourceKey).Replace("\\r\\n", Environment.NewLine);
                }

                return string.Empty;
            }
        }

        public Func<string> AdditionalInfoCallback { get; set; }

        public void UpdateAdditionalInfo()
        {
            RaisePropertyChanged(nameof(AdditionalInfo));
        }
    }
}
