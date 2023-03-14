namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using ConfigWizard;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Door;
    using Kernel;

    [CLSCompliant(false)]
    public class DoorPageViewModel : InspectionWizardViewModelBase
    {
        private readonly IDoorService _door;
        private readonly bool _showMechanicalMeterDoor;

        public DoorPageViewModel(bool isWizard) : base(isWizard)
        {
            _door = ServiceManager.GetInstance().GetService<IDoorService>();
            _showMechanicalMeterDoor = ServiceManager.GetInstance()
                .GetService<IPropertiesManager>()
                .GetValue(ApplicationConstants.ConfigWizardHardMetersConfigVisible, true);
        }

        public ObservableCollection<DoorViewModel> Doors { get; } = new ObservableCollection<DoorViewModel>();

        protected override void OnLoaded()
        {
            Doors.Clear();
            if (!IsWizardPage)
            {
                Access.IgnoreDoors = true;
            }

            foreach (var logicalId in from d in _door.LogicalDoors
                select d.Key)
            {
                var viewModel = new DoorViewModel(Inspection, logicalId);

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

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            foreach (var door in Doors)
            {
                door.OnUnloaded();
            }

            Doors.Clear();

            if (!IsWizardPage)
            {
                Access.IgnoreDoors = false;
            }

            base.OnUnloaded();
        }

        protected override void SetupNavigation()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = true;
            }
        }

        protected override void SaveChanges()
        {
        }
    }
}
