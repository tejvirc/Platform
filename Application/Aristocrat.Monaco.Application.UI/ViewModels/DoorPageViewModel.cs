namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Door;
    using Kernel;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class DoorPageViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IDoorService _door;
        private readonly bool _showMechanicalMeterDoor;

        public DoorPageViewModel()
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
            Access.IgnoreDoors = true;

            foreach (var logicalId in from d in _door.LogicalDoors
                select d.Key)
            {
                var viewModel = new DoorViewModel(logicalId);

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
                    var viewModel = new DoorViewModel(logicalId, true);

                    Doors.Add(viewModel);
                    viewModel.OnLoaded();
                }
            }
        }

        protected override void OnUnloaded()
        {
            foreach (var door in Doors)
            {
                door.OnUnloaded();
            }

            Doors.Clear();

            Access.IgnoreDoors = false;
        }
    }
}
