namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Kernel.Contracts.MessageDisplay;
    using MVVM.Model;

    [CLSCompliant(false)]
    public class StatusMessage : BaseNotify
    {
        private readonly IDisplayableMessage _displayableMessage;

        public StatusMessage(IDisplayableMessage displayableMessage, string additionalInfo)
        {
            _displayableMessage = displayableMessage;
            AdditionalInfo = additionalInfo;
        }

        public string Message => _displayableMessage.Message;

        public string AdditionalInfo { get; set; }
    }
}
