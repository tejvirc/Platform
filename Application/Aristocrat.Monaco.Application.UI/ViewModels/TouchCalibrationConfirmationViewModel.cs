namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Contracts.Input;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Touch;
    using Kernel;
    using MVVM;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class TouchCalibrationConfirmationViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly ISerialTouchCalibration _serialTouchCalibrationService;
        private readonly ITouchCalibration _touchCalibrationService;

        private bool _serialTouchCalibrated;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TouchCalibrationConfirmationViewModel" /> class.
        /// </summary>
        public TouchCalibrationConfirmationViewModel()
        {
            EventBus.Subscribe<TouchCalibrationCompletedEvent>(this, HandleEvent);
 
            _cabinetDetectionService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
            _serialTouchCalibrationService = ServiceManager.GetInstance().GetService<ISerialTouchCalibration>();
            _touchCalibrationService = ServiceManager.GetInstance().GetService<ITouchCalibration>();
        }

        public SystemDownEvent EventHandle { get; set; }

        protected override bool IsModalDialog => true;

        private void InvokeCalibration()
        {
            if (!_serialTouchCalibrated && _cabinetDetectionService.ExpectedDisplayDevicesWithSerialTouch != null)
            {
                if (_serialTouchCalibrationService.IsCalibrating)
                {
                    _serialTouchCalibrationService.CalibrateNextDevice();
                }
                else
                {
                    EventBus.Subscribe<SerialTouchCalibrationCompletedEvent>(this, OnSerialTouchCalibrationCompleted);
                    _serialTouchCalibrationService.BeginCalibration();
                }

                return;
            }

            if (_touchCalibrationService.IsCalibrating)
            {
                _touchCalibrationService.CalibrateNextDevice();
            }
            else
            {
                _touchCalibrationService.BeginCalibration();
            }
        }

        private void OnSerialTouchCalibrationCompleted(SerialTouchCalibrationCompletedEvent e)
        {
            _serialTouchCalibrated = !_serialTouchCalibrationService.IsCalibrating;
            if (_serialTouchCalibrated)
            {
                EventBus.Unsubscribe<SerialTouchCalibrationCompletedEvent>(this);
            }

            InvokeCalibration();
        }

        private void HandleEvent(TouchCalibrationCompletedEvent e)
        {
            Save();
        }

        protected override void HandleEvent(SystemDownEvent downEvent)
        {
            if ((downEvent.LogicalId == (int)ButtonLogicalId.Play ||
                 downEvent.LogicalId == (int)ButtonLogicalId.DualPlay) &&
                downEvent.Enabled == false)
            {
                if (_serialTouchCalibrationService.IsCalibrating)
                {
                    return;
                }

                _serialTouchCalibrated = false;
                MvvmHelper.ExecuteOnUI(InvokeCalibration);
            }
        }
    }
}