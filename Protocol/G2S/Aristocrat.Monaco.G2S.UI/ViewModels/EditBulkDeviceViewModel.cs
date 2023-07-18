namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S.Client;
    using G2S;
    using Kernel;
    using Localization.Properties;

    public class EditBulkDeviceViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IContainerService _containerService = ServiceManager.GetInstance().TryGetService<IContainerService>();
        private readonly IG2SEgm _egm;
        private int _selectedHostId;
        private bool _selectedActive;
        private string _selectedField;
        private List<IHostControl> _hosts;
        private readonly ILocalizer _operatorLocalizer;

        public string SelectedField
        {
            get => _selectedField;

            set
            {
                if (_selectedField != value)
                {
                    _selectedField = value;
                    RaisePropertyChanged(nameof(SelectedField));
                    RaisePropertyChanged(nameof(OwnerVisibility));
                    RaisePropertyChanged(nameof(ActiveVisibility));
                }
            }
        }

        public int SelectedHostId
        {
            get => _selectedHostId;

            set
            {
                if (_selectedHostId != value)
                {
                    _selectedHostId = value;
                    RaisePropertyChanged(nameof(SelectedHostId));
                }
            }
        }

        public bool SelectedActive
        {
            get => _selectedActive;

            set
            {
                if (_selectedActive != value)
                {
                    _selectedActive = value;
                    RaisePropertyChanged(nameof(SelectedActive));
                }
            }
        }

        public Visibility OwnerVisibility =>
            SelectedField == _operatorLocalizer.GetString(ResourceKeys.DeviceManagerOwnerSelection)
                ? Visibility.Visible
                : Visibility.Hidden;

        public Visibility ActiveVisibility =>
            SelectedField == _operatorLocalizer.GetString(ResourceKeys.DeviceManagerActiveSelection)
                ? Visibility.Visible
                : Visibility.Hidden;

        public List<int> HostIds { get; }

        public ObservableCollection<string> EditableFields { get; }

        public EditBulkDeviceViewModel()
        {
            _operatorLocalizer = Localizer.For(CultureFor.Operator);
            _egm = _containerService.Container.GetInstance<IG2SEgm>();
            EventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleOperatorMenuEntered);
            _hosts = new List<IHostControl>();
            HostIds = new List<int>();
            EditableFields = new ObservableCollection<string>
            {
                _operatorLocalizer.GetString(ResourceKeys.DeviceManagerOwnerSelection),
                _operatorLocalizer.GetString(ResourceKeys.DeviceManagerActiveSelection)
            };
            GetHosts();
            HostIds = DistinctHostIDs();
            _selectedHostId = HostIds.FirstOrDefault();
            _selectedField = EditableFields.First();
        }

        private void GetHosts()
        {
            if (_egm != null)
            {
                _hosts = _egm.Hosts.ToList();
            }

        }

        private List<int> DistinctHostIDs()
        {
            return _hosts.Select(h => h.Id).ToList();
        }

        protected override void OnCommitted()
        {
            base.OnCommitted();

            DialogResult = true;
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
