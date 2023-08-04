namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S.Client;
    using Kernel;
    using System.Collections.Generic;
    using System.Linq;
    using Localization.Properties;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using System.ComponentModel.DataAnnotations;

    public class EditDeviceViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IG2SEgm _egm;
        private string _deviceName;
        private int _deviceId;
        private int _ownerId;
        private bool _enabled;
        private bool _hostEnabled;
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
                    OnPropertyChanged(nameof(DeviceName));
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
                    OnPropertyChanged(nameof(DeviceId));
                }
            }
        }

        [CustomValidation(typeof(EditDeviceViewModel), nameof(OwnerIdValidate))]
        public int OwnerId
        {
            get => _ownerId;

            set
            {
                if (_ownerId != value)
                {
                    _ownerId = value;
                    OnPropertyChanged(nameof(OwnerId));
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
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        public bool HostEnabled
        {
            get => _hostEnabled;

            set
            {
                if (_hostEnabled != value)
                {
                    _hostEnabled = value;
                    OnPropertyChanged(nameof(HostEnabled));
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
                    OnPropertyChanged(nameof(Active));
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

        public static ValidationResult OwnerIdValidate(int ownerId, ValidationContext context)
        {
            EditDeviceViewModel instance = (EditDeviceViewModel)context.ObjectInstance;
            var errors = "";
            instance.ClearErrors(nameof(ownerId));
            if (ownerId <= 0)
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostIdGreaterThanZero);
            }
            if (errors == null)
            {
                return ValidationResult.Success;
            }

            return new(errors);
        }

        private void HandleOperatorMenuEntered(IEvent theEvent)
        {
            var operatorMenuEvent = (OperatorMenuEnteredEvent)theEvent;
            if (!operatorMenuEvent.IsTechnicianRole)
            {
                Execute.OnUIThread(Cancel);
            }
        }
    }
}
