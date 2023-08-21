namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Linq;
    using Contracts.MeterPage;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class HardwareMetersPageViewModel : MetersPageViewModelBase
    {
        private int _numTouchScreens;
        private int _numVideoDisplays;

        public HardwareMetersPageViewModel()
            : base(MeterNodePage.Hardware)
        {
        }

        protected override void OnLoaded()
        {
            UpdateData();
            SplitMeters();
            base.OnLoaded();
        }

        private void UpdateData()
        {
            var cabinetDetectionService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
            if (cabinetDetectionService != null)
            {
                _numTouchScreens = cabinetDetectionService.ExpectedTouchDevices.Count();
                _numVideoDisplays = cabinetDetectionService.ExpectedDisplayDevices.Count();
            }
        }
    }
}