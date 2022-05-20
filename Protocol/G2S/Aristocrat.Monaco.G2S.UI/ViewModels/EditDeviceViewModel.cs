namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S.Client;
    using Kernel;
    using MVVM;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Localization;
    using Localization.Properties;

    public class EditDeviceViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IG2SEgm _egm;
        private string _deviceName;
        private int _deviceId;
        private int _ownerId;
        private bool _enabled;
        private bool _active;
        private List<IHostControl> _hosts;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditHostViewModel" /> class.
        /// </summary>
        public EditDeviceViewModel()
        {
            var container = ServiceManager.GetInstance().TryGetService<IContainerService>();
            _egm = container?.Container?.GetInstance<IG2SEgm>();
            EventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleOperatorMenuEntered);
            HostIds = new List<int>();
            GetHosts();
            HostIds = DistinctHostIDs();
        }

        public string DeviceName
        {
            get => _deviceName;

            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    RaisePropertyChanged(nameof(DeviceName));
                }
            }
        }

        public int DeviceId
        {
            get => _deviceId;

            set
            {
                if (_deviceId != value)
                {
                    _deviceId = value;
                    RaisePropertyChanged(nameof(DeviceId));
                }
            }
        }

        public int OwnerId
        {
            get => _ownerId;

            set
            {
                if (_ownerId != value)
                {
                    _ownerId = value;
                    RaisePropertyChanged(nameof(OwnerId));
                }
            }
        }

        public bool Enabled
        {
            get => _enabled;

            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    RaisePropertyChanged(nameof(Enabled));
                }
            }
        }

        public bool Active
        {
            get => _active;

            set
            {
                if (_active != value)
                {
                    _active = value;
                    RaisePropertyChanged(nameof(Active));
                }
            }
        }

        public List<int> HostIds { get; }

        private List<int> DistinctHostIDs()
        {
            return _hosts.Select(h => h.Id).Distinct().ToList();
        }

        private void GetHosts()
        {
            if (_egm != null)
            {
                _hosts = _egm.Hosts.ToList();
            }

        }

        /// <inheritdoc />
        protected override void OnCommitted()
        {
            base.OnCommitted();

            DialogResult = true;
        }

        /// <inheritdoc />
        protected override void ValidateAll()
        {
            base.ValidateAll();
            ValidateHostId(OwnerId);
        }

        private void ValidateHostId(int ownerId)
        {
            ClearErrors(nameof(OwnerId));
            if (ownerId <= 0)
            {
                SetError(nameof(OwnerId), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostIdGreaterThanZero));
            }
        }

        private void HandleOperatorMenuEntered(IEvent theEvent)
        {
            var operatorMenuEvent = (OperatorMenuEnteredEvent)theEvent;
            if (!operatorMenuEvent.IsTechnicianRole)
            {
                MvvmHelper.ExecuteOnUI(Cancel);
            }
        }
    }
}