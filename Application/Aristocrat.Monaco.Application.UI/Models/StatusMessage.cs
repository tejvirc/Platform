namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Kernel;

    [CLSCompliant(false)]
    public class StatusMessage : ObservableObject
    {
        private readonly DisplayableMessage _displayableMessage;

        public StatusMessage(DisplayableMessage displayableMessage, string additionalInfo)
        {
            _displayableMessage = displayableMessage;
            AdditionalInfo = additionalInfo;
        }

        public string Message => _displayableMessage.Message;

        public string AdditionalInfo { get; set; }
    }
}
