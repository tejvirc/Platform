namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Diagnostics;
    using OperatorMenu;

    [CLSCompliant(false)]
    public sealed class HardwareMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/HardwareMenu";
        private const string TouchScreenCalibrationExe = "twcalib.exe";

        private volatile bool _calibrating;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareMainPageViewModel" /> class.
        /// </summary>
        public HardwareMainPageViewModel(string pageNameResourceKey) : base(pageNameResourceKey, MenuExtensionPointPath)
        {
        }

        public override int MinButtonWidth => 100;

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            // A small dialog may be still not closed. For instance,
            // if the touch screen is not connected properly, one
            // dialog may show up and the operator may forget to close
            // when exiting the menu.
            CloseTouchScreenCalibrationProcess();
        }

        private void CloseTouchScreenCalibrationProcess()
        {
            if (_calibrating)
            {
                var touchScreenCalibrations = Process.GetProcessesByName(TouchScreenCalibrationExe.Split('.')[0]);
                if (touchScreenCalibrations.Length > 0)
                {
                    foreach (var each in touchScreenCalibrations)
                    {
                        each.CloseMainWindow();
                    }
                }
            }

            _calibrating = false;
        }
    }
}
