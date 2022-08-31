namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Linq;
    using ConfigWizard;
    using Contracts.Input;
    using Contracts.Localization;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class TouchCalibrationErrorViewModel : OperatorMenuSaveViewModelBase, IConfigWizardDialog
    {
        private readonly int _touchscreenCount;
        private readonly ITouchCalibration _calibrationService;

        public TouchCalibrationErrorViewModel(string errorText = "")
        {
            EventBus.Subscribe<SystemDownEvent>(this, HandleEvent);
            EventBus.Subscribe<TouchCalibrationCompletedEvent>(this, HandleEvent);

            _touchscreenCount = ServiceManager.GetInstance().GetService<ICabinetDetectionService>()
                .ExpectedTouchDevices.Count();

            _calibrationService = ServiceManager.GetInstance().GetService<ITouchCalibration>();

            if (string.IsNullOrEmpty(errorText))
            {
                ErrorText = string.Format(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchCalibrationErrorText),
                    _touchscreenCount);
            }
            else
            {
                ErrorText = errorText;
            }
        }

        public string ErrorText { get; }

        public bool IsInWizard { get; set; }

        public SystemDownEvent EventHandle { get; set; }

        protected override bool IsModalDialog => true;

        public void Close()
        {
            Cancel();
        }

        private void InvokeCalibration()
        {
            if (_calibrationService.IsCalibrating)
            {
                _calibrationService.CalibrateNextDevice();
            }
            else
            {
                _calibrationService.BeginCalibration();
            }
        }

        private void HandleEvent(TouchCalibrationCompletedEvent calibrationCompletedEvent)
        {
            Save();
        }

        private void HandleEvent(SystemDownEvent downEvent)
        {
            if (downEvent.LogicalId == (int)ButtonLogicalId.Play && downEvent.Enabled == false)
            {
                MvvmHelper.ExecuteOnUI(InvokeCalibration);
            }
        }
    }
}