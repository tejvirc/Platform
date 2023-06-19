namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using MVVM;
    using ConfigWizard;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Door;
    using Kernel;
    using Kernel.Contracts;

    [CLSCompliant(false)]
    public class DoorPageViewModel : InspectionWizardViewModelBase
    {
        private readonly IDoorService _door;
        private readonly bool _showMechanicalMeterDoor;
        private readonly bool _requireOpticSensors;
        private readonly bool _requireBellyDoor;

        public DoorPageViewModel(bool isWizard) : base(isWizard)
        {
            _door = ServiceManager.GetInstance().GetService<IDoorService>();
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _showMechanicalMeterDoor = properties
                .GetValue(ApplicationConstants.ConfigWizardHardMetersConfigVisible, true);

            if (properties.GetValue(KernelConstants.IsInspectionOnly, false))
            {
                _requireOpticSensors = properties.GetValue(ApplicationConstants.ConfigWizardDoorOpticsEnabled, false)
                    && properties.GetValue(ApplicationConstants.ConfigWizardDoorOpticsVisible, false);
                _requireBellyDoor = properties.GetValue(ApplicationConstants.ConfigWizardBellyPanelDoorEnabled, false)
                    && properties.GetValue(ApplicationConstants.ConfigWizardBellyPanelDoorVisible, false);
            }
        }

        public ObservableCollection<DoorViewModel> Doors { get; } = new ObservableCollection<DoorViewModel>();

        protected override void OnLoaded()
        {
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, HandleOperatorCultureChanged);

            LoadDoors();

            base.OnLoaded();
        }

        private void HandleOperatorCultureChanged(OperatorCultureChangedEvent @event)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                ClearDoors();
                LoadDoors();
            });
        }

        protected override void OnUnloaded()
        {
            ClearDoors();
            base.OnUnloaded();
        }

        protected override void SetupNavigation()
        {
            UpdateNavigation();
        }

        protected override void SaveChanges()
        {
        }

        private void UpdateNavigation()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = Doors.All(d => d.IsTestPassed);
            }
        }

        private void OnDoorPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(DoorViewModel.IsTestPassed))
            {
                return;
            }

            UpdateNavigation();
        }

        private void ClearDoors()
        {
            foreach (var door in Doors)
            {
                door.PropertyChanged -= OnDoorPropertyChanged;
                door.OnUnloaded();
            }

            Doors.Clear();

            if (!IsWizardPage)
            {
                Access.IgnoreDoors = false;
            }
        }

        private void LoadDoors()
        {
            Doors.Clear();
            if (!IsWizardPage)
            {
                Access.IgnoreDoors = true;
            }

            foreach (var logicalId in _door.LogicalDoors.Keys)
            {
                var requiredDoor = ((logicalId == (int)DoorLogicalId.MainOptic || logicalId == (int)DoorLogicalId.TopBoxOptic) && _requireOpticSensors)
                    || (logicalId == (int)DoorLogicalId.Belly && _requireBellyDoor);
                var viewModel = new DoorViewModel(Inspection, logicalId, false, requiredDoor);

                Doors.Add(viewModel);
                viewModel.OnLoaded();
            }

            if (GetConfigSetting(OperatorMenuSetting.ShowUnconfiguredDoors, true))
            {
                if (!_showMechanicalMeterDoor)
                {
                    _door.IgnoredDoors.Remove((int)DoorLogicalId.MechanicalMeter);
                }

                foreach (var logicalId in _door.IgnoredDoors)
                {
                    var viewModel = new DoorViewModel(Inspection, logicalId, true);

                    Doors.Add(viewModel);
                    viewModel.OnLoaded();
                }
            }

            foreach (var door in Doors)
            {
                door.PropertyChanged += OnDoorPropertyChanged;
            }
        }
    }
}
