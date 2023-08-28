namespace Aristocrat.Monaco.G2S.UI.Models
{
    using System;
    using Aristocrat.G2S.Protocol.v21;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Monaco.Application.Contracts.Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Comms info model
    /// </summary>
    public class CommsInfo : ObservableObject
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
                    OnPropertyChanged(nameof(HostId));
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
                    OnPropertyChanged(nameof(Address));
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
                    OnPropertyChanged(nameof(Registered));
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
                    OnPropertyChanged(nameof(OutboundOverflow));
                }
                OnPropertyChanged(nameof(OutboundOverflowText));
            }
        }

        public string OutboundOverflowText
        {
            get => Localizer.For(CultureFor.Operator).GetString(OutboundOverflow ? ResourceKeys.TrueText : ResourceKeys.FalseText);
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
                    OnPropertyChanged(nameof(InboundOverflow));
                }
                OnPropertyChanged(nameof(InboundOverflowText));
            }
        }

        public string InboundOverflowText
        {
            get => Localizer.For(CultureFor.Operator).GetString(InboundOverflow ? ResourceKeys.TrueText : ResourceKeys.FalseText);
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
                    OnPropertyChanged(nameof(TransportState));
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
                    OnPropertyChanged(nameof(State));
                }
            }
        }
    }
}