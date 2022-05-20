namespace Aristocrat.Monaco.G2S.UI.Models
{
    using Aristocrat.G2S.Protocol.v21;
    using System;
    using MVVM.Model;

    /// <summary>
    ///     Comms info model
    /// </summary>
    public class CommsInfo : BaseNotify
    {
        private Uri _address;
        private int _hostId;
        private bool _inboundOverflow;
        private bool _outboundOverflow;
        private bool _registered;
        private t_commsStates _state;
        private t_transportStates _transportState;

        /// <summary>
        ///     Gets or sets the index
        /// </summary>
        public int Index { get; set; }

        /// <summary> Gets or sets the Id </summary>
        public int HostId
        {
            get => _hostId;

            set
            {
                if (_hostId != value)
                {
                    _hostId = value;
                    RaisePropertyChanged(nameof(HostId));
                }
            }
        }

        /// <summary> Gets or sets the Host Address </summary>
        public Uri Address
        {
            get => _address;

            set
            {
                if (_address != value)
                {
                    _address = value;
                    RaisePropertyChanged(nameof(Address));
                }
            }
        }

        /// <summary> Gets or sets a value indicating whether the host is registered </summary>
        public bool Registered
        {
            get => _registered;

            set
            {
                if (_registered != value)
                {
                    _registered = value;
                    RaisePropertyChanged(nameof(Registered));
                }
            }
        }

        /// <summary> Gets or sets a value indicating whether the OutboundOverflow is set.</summary>
        public bool OutboundOverflow
        {
            get => _outboundOverflow;

            set
            {
                if (_outboundOverflow != value)
                {
                    _outboundOverflow = value;
                    RaisePropertyChanged(nameof(OutboundOverflow));
                }
            }
        }

        /// <summary> Gets or sets a value indicating whether the InboundOverflow is set.</summary>
        public bool InboundOverflow
        {
            get => _inboundOverflow;

            set
            {
                if (_inboundOverflow != value)
                {
                    _inboundOverflow = value;
                    RaisePropertyChanged(nameof(InboundOverflow));
                }
            }
        }

        /// <summary> Gets or sets the TransportState </summary>
        public t_transportStates TransportState
        {
            get => _transportState;

            set
            {
                if (_transportState != value)
                {
                    _transportState = value;
                    RaisePropertyChanged(nameof(TransportState));
                }
            }
        }

        /// <summary> Gets or sets the State </summary>
        public t_commsStates State
        {
            get => _state;

            set
            {
                if (_state != value)
                {
                    _state = value;
                    RaisePropertyChanged(nameof(State));
                }
            }
        }
    }
}