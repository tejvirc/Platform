namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Aristocrat.G2S.Client;
    using G2S;
    using Kernel;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using Toolkit.Mvvm.Extensions;

    public class EditBulkDeviceViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IContainerService _containerService = ServiceManager.GetInstance().TryGetService<IContainerService>();
        private readonly IG2SEgm _egm;
        private int _selectedHostId;
        private bool _selectedActive;
        private string _selectedField;
        private List<IHostControl> _hosts;

        public string SelectedField
        {
            get => _selectedField;

            set
            {
                if (_selectedField != value)
                {
                    _selectedField = value;
                    OnPropertyChanged(nameof(SelectedField));
                    OnPropertyChanged(nameof(OwnerVisibility));
                    OnPropertyChanged(nameof(ActiveVisibility));
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
                    OnPropertyChanged(nameof(SelectedHostId));
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
                    OnPropertyChanged(nameof(SelectedActive));
                }
            }
        }

        public Visibility OwnerVisibility => SelectedField == "Owner" ? Visibility.Visible : Visibility.Hidden;

        public Visibility ActiveVisibility => SelectedField == "Active" ? Visibility.Visible : Visibility.Hidden;

        public List<int> HostIds { get; }
        public ObservableCollection<string> EditableFields { get; }

        public EditBulkDeviceViewModel()
        {
            _egm = _containerService.Container.GetInstance<IG2SEgm>();
            EventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleOperatorMenuEntered);
            _hosts = new List<IHostControl>();
            HostIds = new List<int>();
            EditableFields = new ObservableCollection<string> {"Owner", "Active"};
            GetHosts();
            HostIds = DistinctHostIDs();
            _selectedHostId = HostIds.FirstOrDefault();
            _selectedField = "Owner";
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
                Execute.OnUIThread(Cancel);
            }
        }
    }
}
