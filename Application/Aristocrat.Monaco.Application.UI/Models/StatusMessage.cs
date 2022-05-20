namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Kernel;
    using MVVM.Model;

    [CLSCompliant(false)]
    public class StatusMessage : BaseNotify
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
