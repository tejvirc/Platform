namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using MVVM.Model;

    [CLSCompliant(false)]
    public class ProtocolSelection : BaseNotify
    {
        private bool _selected;
        private bool _enabled;
        private string _protocolName;

        public void Initialize(string protocolName)
        {
            ProtocolName = protocolName;
            Selected = false;
            Enabled = true;
        }

        public string ProtocolName
        {
            get => _protocolName;
            set
            {
                if (value == _protocolName)
                {
                    return;
                }

                SetProperty(ref _protocolName, value, nameof(ProtocolName));
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (value == _selected)
                {
                    return;
                }

                SetProperty(ref _selected, value, nameof(Selected));
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled)
                {
                    return;
                }

                SetProperty(ref _enabled, value, nameof(Enabled));
            }
        }
    }
}
